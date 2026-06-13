using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.SessionStore;
using JOSYN.Commons.Helpers;
using JOSYN.Commons.Log;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using System.Diagnostics;
using System.Text;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    internal static async Task<int> Run(string[] args)
    {
#if DEBUG
        Console.InputEncoding = new UTF8Encoding();
        Console.OutputEncoding = new UTF8Encoding();
        LocalLog.EnableConsoleOutput = true;
#endif
        var loadConfig = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName));
        if (!loadConfig.Succeeded)
        {
            LocalLog.WriteError(loadConfig.ErrorMessage);
            // ReSharper disable once MethodHasAsyncOverload
            Console.Error.WriteLine($"Bootstrap-Konfiguration konnte nicht geladen werden: {loadConfig.ErrorMessage}");
            return 1;
        }
        var config = loadConfig.Value;
        var errorHandler = new SqlErrorHandler(config.SessionStoreConnectionString);

        try
        {
#if DEBUG
            Console.WriteLine("ARGS: " + string.Join(" | ", args));
#endif
            // Mode dispatch: JOSYN-START or JOSYN-IPC
            if (args.Length >= 2 && args[0] == "JOSYN-START")
                return await HandleSessionStart(args[1], config, errorHandler);

            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
            {
                const string msg = "Keine IPC-Session-UID angegeben.";
                errorHandler.Handle(msg, null, null);
                return 1;
            }

            var sessionStore = new SessionStore(config.SessionStoreConnectionString);

            var getSource = ResolveConfigSource(config);
            if (!getSource.Succeeded)
            {
                var err = getSource.ToResult();
                errorHandler.Handle(err, sessionGuid: null);
                return 1;
            }
            var configStore = new ConfigStore(getSource.Value, config.SessionStoreConnectionString);

            return await RunServer(sessionKey, sessionStore, configStore, config, errorHandler);
        }
        catch (Exception ex)
        {
            const string msg = "Unbehandelte Exception im Host.";
            errorHandler.Handle(msg, callStack: null, exceptionDetails: ex.ToString());
            return 1;
        }
        finally
        {
#if DEBUG
            Console.Write("\n[PRESS ANY KEY TO EXIT...]");
            Console.ReadKey(true);
#endif
        }
    }

    // -------------------------------------------------------------------------
    // JOSYN-START mode
    // -------------------------------------------------------------------------

    private static async Task<int> HandleSessionStart(
        string fileArg, IBootstrapConfig config, IErrorHandler errorHandler)
    {
        if (!fileArg.StartsWith('@'))
        {
            errorHandler.Handle("JOSYN-START: Dateiargument muss mit '@' beginnen.", null, null);
            return 1;
        }

        var filePath = fileArg[1..];
        string rawRequest;
        try
        {
            rawRequest = File.ReadAllText(filePath);
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            errorHandler.Handle(
                $"JOSYN-START: Temp-Datei konnte nicht gelesen werden: '{filePath}'",
                callStack: null, exceptionDetails: ex.ToString());
            return 1;
        }

        var deserialize = PropertyBag.Deserialize<SessionStartRequest>(rawRequest);
        if (!deserialize.Succeeded)
        {
            errorHandler.Handle(
                $"JOSYN-START: SessionStartRequest konnte nicht deserialisiert werden: {deserialize.ErrorMessage}",
                callStack: null, exceptionDetails: null);
            return 1;
        }
        var request = deserialize.Value;

        string decodedArguments;
        try
        {
            decodedArguments = Encoding.UTF8.GetString(Convert.FromBase64String(request.Arguments));
        }
        catch (Exception ex)
        {
            errorHandler.Handle(
                "JOSYN-START: Arguments-Feld konnte nicht base64-dekodiert werden.",
                callStack: null, exceptionDetails: ex.ToString());
            return 1;
        }

        var sessionStore = new SessionStore(config.SessionStoreConnectionString);
        var jobName      = request.JobTypeName;

        // Turnstile scope: GUID allocation + session persistence (ADR-007).
        // The accept/reject negotiation extends the effective concurrency window via
        // the 'preparing' status — GetConcurrentSessionArguments includes 'preparing'
        // sessions so that concurrent starts see each other even before Running (ADR-008).
        Guid sessionGuid = Guid.Empty;
        var turnstileResult = Turnstile.Run(jobName, () =>
        {
            sessionGuid = Guid.NewGuid();
            var save = sessionStore.SaveNewSession(new JobSessionRecord
            {
                UID               = sessionGuid,
                JobTypeName       = jobName,
                Arguments         = decodedArguments,
                Result            = string.Empty,
                JobVersion        = string.Empty,
                UserName          = request.CallerUser,
                UserDomain        = request.CallerDomain,
                ClientApplication = request.CallerApplication,
                ClientMachine     = request.CallerMachine,
                TecUser           = request.TechnicalUserName,
                Started           = DateTime.Now,
                ExecutionStatus   = ExecutionStatus.Pending,
                JapServerProcess  = 0,
                JobHostProcessId  = 0,
                JapExitCode       = 0,
                JobExitCode       = 0
            });
            if (!save.Succeeded)
                throw new InvalidOperationException(save.ErrorMessage);
        });

        if (!turnstileResult.Succeeded)
        {
            errorHandler.Handle(
                turnstileResult.ErrorMessage!,
                callStack: null, exceptionDetails: null,
                sessionGuid: sessionGuid == Guid.Empty ? null : sessionGuid);
            return 1;
        }

        // Build job exe path (needed for both JobVersion read and process spawn).
        var jobExePath = Path.Combine(config.BackendRoot, "JobRepository", jobName, jobName + ".exe");

        // Transition to Preparing and record JobVersion in one write.
        SetPreparingWithVersion(sessionStore, sessionGuid, jobExePath, errorHandler, jobName);

        var getSource = ResolveConfigSource(config);
        if (!getSource.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(getSource.ToResult(), sessionGuid: sessionGuid);
            return 1;
        }
        var configStore = new ConfigStore(getSource.Value, config.SessionStoreConnectionString);

        var japServer = new JAPServer(sessionStore, sessionGuid, jobName, errorHandler, configStore);
        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(japServer);
        if (!dispatcherResult.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(dispatcherResult.ToResult(), jobName: jobName, sessionGuid: sessionGuid);
            return 1;
        }
        var jipDispatcher = dispatcherResult.Value;

        // Start serve loop (spawns job.exe + handles all JAP requests including negotiation).
        var shouldCancelServer = false;
        var serverStartArguments = new ServerStartArguments
        {
            ClientExePath           = jobExePath,
            ConnectionTimeout       = TimeSpan.FromMinutes(1),
            HandleStringRequest     = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey              = sessionGuid,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, sessionGuid, errorHandler),
            IsCancellationRequested = () => Task.FromResult(shouldCancelServer)
        };

        var serverTask = PipesServer.RunAsync(serverStartArguments);

        // Await negotiation outcome with 30-second timeout (ADR-008).
        var winner = await Task.WhenAny(japServer.NegotiationOutcome, Task.Delay(TimeSpan.FromSeconds(30)));

        if (winner != japServer.NegotiationOutcome)
        {
            // Timeout — job.exe never called AcceptSession or RejectSession.
            LocalLog.WriteError($"Negotiation timeout for session '{sessionGuid}' ({jobName}) — treating as rejected.");
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedRejected, errorHandler, jobName);
            shouldCancelServer = true;
            await serverTask;
            return 0;
        }

        if (!japServer.NegotiationOutcome.Result)
        {
            // Rejected — JAPServer.RejectSession already set FinishedRejected + Finished.
            shouldCancelServer = true;
            await serverTask;
            return 0;
        }

        // Accepted — JAPServer.AcceptSession already set Running.
        var res = await serverTask;

        if (!res.Succeeded)
        {
            if (!japServer.TerminalStatusSet)
                SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(res, jobName: jobName, sessionGuid: sessionGuid);
            return 1;
        }

        if (!japServer.TerminalStatusSet)
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedSuccessfully, errorHandler, jobName);

        LocalLog.WriteInfo("Server terminiert.");
        return 0;
    }

    // -------------------------------------------------------------------------

    private static void SetPreparingWithVersion(
        SessionStore sessionStore, Guid sessionGuid, string jobExePath,
        IErrorHandler errorHandler, string jobName)
    {
        var version = string.Empty;
        try { version = FileVersionInfo.GetVersionInfo(jobExePath).ProductVersion ?? string.Empty; }
        catch { /* exe not found or version unreadable — version stays empty */ }

        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with
        {
            ExecutionStatus = ExecutionStatus.Preparing,
            JobVersion      = version
        };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }
}
