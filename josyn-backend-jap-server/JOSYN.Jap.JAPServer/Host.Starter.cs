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
    private const string JobRepositoryFolder = "JobRepository";

    internal static async Task<int> Run(string[] args)
    {
        var bootStrapConfig = LoadBootstrapConfig();
        if (bootStrapConfig == null) return 1;
        
        var errorHandler = new SqlErrorHandler(bootStrapConfig.SessionStoreConnectionString);
        // ReSharper disable once InvertIf
        if (args.Length < 2 || args[0] != JapServerConstants.CliModeStart)
        {
            const string err = "Unbekannter oder fehlender Start-Modus. Erwartet: JOSYN-START @<path>";
            errorHandler.Handle(err, null, null);
            return 1;
        }
        
        var sessionStartSpecFilepath = args[1];
        return await ProcessSessionStart(sessionStartSpecFilepath, bootStrapConfig, errorHandler);
    }

    /// <summary>
    /// Verarbeitet den JOSYN-START-Modus: Liest die SessionStartSpec aus der übergebenen Temp-Datei, führt die Turnstile-Logik für die Session-Akzeptanz durch, startet den JAPServer und verhandelt die Parallel-Execution-Allowance mit dem Job.exe.
    /// </summary>
    /// <param name="sessionStartSpecFilepath"></param>
    /// <param name="bootStrapConfig"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
#pragma warning disable CA1859
    private static async Task<int> ProcessSessionStart(string sessionStartSpecFilepath, IBootstrapConfig bootStrapConfig, IErrorHandler errorHandler)
#pragma warning restore CA1859
    {
        var getSpec = GetSessionStartSpec(sessionStartSpecFilepath);
        if (!getSpec.Succeeded)
        {
            errorHandler.Handle(getSpec.ToResult());
            return 1;
        }
        var startSpec = getSpec.Value;
        
        
        var getPlainArguments = Base64DecodeArgumentsString(startSpec.Arguments);
        if (!getPlainArguments.Succeeded)
        {
            errorHandler.Handle(getPlainArguments.ToResult());
            return 1;
        }
        
        
        var sessionStore = new SessionStore(bootStrapConfig.SessionStoreConnectionString);
        var jobName = startSpec.JobTypeName;
        var jobExePath = Path.Combine(bootStrapConfig.BackendRoot, JobRepositoryFolder, jobName, jobName + ".exe");

        // Turnstile scope: GUID allocation → session persistence → spawn → accept/reject negotiation.
        // Released only when the session is definitively in-flight (running) or closed (finished-rejected).
        // ADR-017B-01 §4, ADR-018B-01 §6.

        var sessionGuid = Guid.Empty;
        var shouldCancelServer = false;
        Task<Result>? serverTask = null;
        JAPServer? japServer = null;
        var negotiationAccepted = false;
        var innerError = Result.Success;

        var turnstileResult = await Turnstile.RunAsync(jobName, async () =>
        {
            sessionGuid = Guid.NewGuid();
            var save = sessionStore.SaveNewSession(new JobSessionRecord
            {
                UID = sessionGuid,
                JobTypeName = jobName,
                Arguments = getPlainArguments.Value,
                Result = string.Empty,
                JobVersion = string.Empty,
                UserName = startSpec.CallerUser,
                UserDomain = startSpec.CallerDomain,
                ClientApplication = startSpec.CallerApplication,
                ClientMachine = startSpec.CallerMachine,
                TecUser = startSpec.TechnicalUserName,
                Started = DateTime.Now,
                ExecutionStatus = ExecutionStatus.Pending,
                JapServerProcess = 0,
                JobHostProcessId = 0,
                JapExitCode = 0,
                JobExitCode = 0
            });
            if (!save.Succeeded)
            {
                sessionGuid = Guid.Empty; // record was never persisted — no valid session to reference
                innerError = save;
                return;
            }

            SetPreparingWithVersion(sessionStore, sessionGuid, jobExePath, errorHandler, jobName);

            var getSource = ResolveConfigSource(bootStrapConfig);
            if (!getSource.Succeeded)
            {
                SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
                innerError = getSource.ToResult();
                return;
            }
            var configStore = new ConfigStore(getSource.Value, bootStrapConfig.SessionStoreConnectionString);

            japServer = new JAPServer(sessionStore, sessionGuid, jobName, errorHandler, configStore);
            var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(japServer);
            if (!dispatcherResult.Succeeded)
            {
                SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
                innerError = dispatcherResult.ToResult();
                return;
            }
            var jipDispatcher = dispatcherResult.Value;

            var serverStartArguments = new ServerStartArguments
            {
                ClientExePath = jobExePath,
                ConnectionTimeout = TimeSpan.FromMinutes(1),
                HandleStringRequest = requestStr => HandleRequest(jipDispatcher, requestStr),
                SessionKey = sessionGuid,
                HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, sessionGuid, errorHandler),
                IsCancellationRequested = () => Task.FromResult(shouldCancelServer)
            };

            serverTask = PipesServer.RunAsync(serverStartArguments);

            // Await negotiation outcome with 30-second timeout (ADR-018B-01 §5).
            var winner = await Task.WhenAny(japServer.NegotiationOutcome, Task.Delay(TimeSpan.FromSeconds(30)));

            if (winner != japServer.NegotiationOutcome)
            {
                // Timeout — job.exe never called AcceptSession or RejectSession.
                LocalLog.WriteError($"Negotiation timeout for session '{sessionGuid}' ({jobName}) — treating as rejected.");
                SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedRejected, errorHandler, jobName);
                shouldCancelServer = true;
                await serverTask;
                serverTask = null;
                return;
            }

            if (!japServer.NegotiationOutcome.Result)
            {
                // Rejected — JAPServer.RejectSession already set FinishedRejected.
                shouldCancelServer = true;
                await serverTask;
                serverTask = null;
                return;
            }

            // Accepted — JAPServer.AcceptSession already set Running.
            // Turnstile releases here: session is definitively in-flight.
            negotiationAccepted = true;
        });

        if (!turnstileResult.Succeeded)
        {
            errorHandler.Handle(
                turnstileResult.ErrorMessage!,
                callStack: null, exceptionDetails: null,
                sessionGuid: sessionGuid == Guid.Empty ? null : sessionGuid);
            return 1;
        }

        if (!innerError.Succeeded)
        {
            errorHandler.Handle(innerError, sessionGuid: sessionGuid);
            return 1;
        }

        if (!negotiationAccepted)
            return 0;

        // Session accepted and running — await JAP serve loop.
        var res = await serverTask!;

        if (!res.Succeeded)
        {
            if (!japServer!.TerminalStatusSet)
                SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(res, jobName: jobName, sessionGuid: sessionGuid);
            return 1;
        }

        if (!japServer!.TerminalStatusSet)
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedSuccessfully, errorHandler, jobName);

        LocalLog.WriteInfo("Server terminiert.");
        return 0;
    }

    #region Konsolidiert
    
    private static FileBootstrapConfig? LoadBootstrapConfig()
    {
        //
        // Bootstrapping -> noch keine ErrorHandler, also Fehler direkt loggen und mit null zurückgeben. 
        //
        var loadConfig = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName));
        if (loadConfig.Succeeded) return loadConfig.Value;
        var err = $"Bootstrap-Konfiguration konnte nicht geladen werden: {loadConfig.ErrorMessage}";
        LocalLog.WriteError(err);   
        return null;
    }

    private static Result<SessionStartSpec> GetSessionStartSpec(string filePathArgument)
    {
        var readRawStartSpec = ReadRawSessionStartSpec(filePathArgument);
        if (!readRawStartSpec.Succeeded)
            return Result<SessionStartSpec>.Propagate(readRawStartSpec.ToResult<SessionStartSpec>());

        var getSpec = DeserializeSessionStartSpec(readRawStartSpec.Value);
        return getSpec;
    }

    private static Result<string> ReadRawSessionStartSpec(string filePathArgument)
    {
        try
        {
            if (!filePathArgument.StartsWith('@'))
                return Result<string>.Fail("JOSYN-START: Dateiargument muss mit '@' beginnen.");
            
            var filePath = filePathArgument[1..];
            var raw = File.ReadAllText(filePath);
            try
            {
                File.Delete(filePath);
            }
            catch { /* ignore */ }

            return raw;
        }
        catch (Exception ex) 
        { 
            return Result<string>.Fail($"JOSYN-START: SessionStartSpec-Datei konnte nicht gelesen werden: '{filePathArgument}'", ex);
        }
    }
   
    private static Result<SessionStartSpec> DeserializeSessionStartSpec(string raw)
    {
        var deserialize = PropertyBag.Deserialize<SessionStartSpec>(raw);
        if (deserialize.Succeeded) return deserialize.Value;
        return Result<SessionStartSpec>.Propagate(deserialize.ToResult<SessionStartSpec>());
    }
    
    private static Result<string> Base64DecodeArgumentsString(string base64String)
    {
        try
        {
            var result = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
            return result;
        }
        catch (Exception ex) { return ex; }
    }

    #endregion

    private static void SetPreparingWithVersion(SessionStore sessionStore, Guid sessionGuid, string jobExePath, IErrorHandler errorHandler, string jobName)
    {
        var version = string.Empty;
        try { version = FileVersionInfo.GetVersionInfo(jobExePath).ProductVersion ?? string.Empty; }
        catch { /* exe not found or version unreadable — version stays empty */ }

        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with
        {
            ExecutionStatus = ExecutionStatus.Preparing,
            JobVersion = version
        };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }
}
