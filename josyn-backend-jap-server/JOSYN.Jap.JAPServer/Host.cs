using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.SessionStore;
using JOSYN.Backend.Contracts;
using JOSYN.Commons.Helpers;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using JOSYN.Commons.Log;
using System.Diagnostics;
using System.Text;

namespace JOSYN.Jap.JAPServer;

internal static class Host
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
            string msg = "Unbehandelte Exception im Host.";
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
            decodedArguments = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Arguments));
        }
        catch (Exception ex)
        {
            errorHandler.Handle(
                "JOSYN-START: Arguments-Feld konnte nicht base64-dekodiert werden.",
                callStack: null, exceptionDetails: ex.ToString());
            return 1;
        }

        var sessionStore = new SessionStore(config.SessionStoreConnectionString);

        // Turnstile scope: GUID allocation + session persistence.
        // ADR-007: scope must also cover job spawn + accept/reject negotiation (ADR-008).
        // Extend this block when ADR-008 is implemented.
        Guid sessionGuid = Guid.Empty;
        var turnstileResult = Turnstile.Run(request.JobTypeName, () =>
        {
            sessionGuid = Guid.NewGuid();
            var save = sessionStore.SaveNewSession(new JobSessionRecord
            {
                UID               = sessionGuid,
                JobTypeName       = request.JobTypeName,
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

        var getSource = ResolveConfigSource(config);
        if (!getSource.Succeeded)
        {
            errorHandler.Handle(getSource.ToResult(), sessionGuid: sessionGuid);
            return 1;
        }
        var configStore = new ConfigStore(getSource.Value, config.SessionStoreConnectionString);

        return await RunServer(sessionGuid, sessionStore, configStore, config, errorHandler);
    }

    // -------------------------------------------------------------------------
    // Config source resolution
    // -------------------------------------------------------------------------

#pragma warning disable CA1859
    private static Result<IConfigSource> ResolveConfigSource(IBootstrapConfig config)
#pragma warning restore CA1859
    {
        if (config.ConfigSourceType is null)
            return new SqlConfigSource(config.SessionStoreConnectionString);

        return LoadAdapterConfigSource(config.ConfigSourceType);
    }

    private static Result<IConfigSource> LoadAdapterConfigSource(string typeName)
    {
        try
        {
            var parts = typeName.Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                return Result.Error(
                    $"Ungültiger ConfigSourceType-Wert: '{typeName}'. " +
                    $"Erwartet: 'FullTypeName, AssemblyName'");

            var assemblyFileName = parts[1] + ".dll";
            var adaptersFolder   = Path.Combine(AppContext.BaseDirectory, "Adapters");
            var assemblyPath     = Path.Combine(adaptersFolder, assemblyFileName);

            if (!File.Exists(assemblyPath))
                return Result.Error(
                    $"Adapter-Assembly nicht gefunden: '{assemblyPath}'. " +
                    $"Stelle sicher, dass die Assembly im 'Adapters/'-Ordner liegt.");

            var alc      = new AdapterLoadContext(assemblyPath);
            var assembly = alc.LoadFromAssemblyPath(assemblyPath);
            var type     = assembly.GetType(parts[0]);

            if (type is null)
                return Result.Error(
                    $"Adapter-Typ '{parts[0]}' nicht gefunden in '{assemblyPath}'.");

            if (Activator.CreateInstance(type) is not IConfigSource source)
                return Result.Error(
                    $"Typ '{parts[0]}' implementiert IConfigSource nicht.");

            return Result<IConfigSource>.Success(source);
        }
        catch (Exception ex) { return ex; }
    }

    // -------------------------------------------------------------------------
    // Server lifecycle
    // -------------------------------------------------------------------------

    private static async Task<int> RunServer(
        Guid sessionKey, SessionStore sessionStore, ConfigStore configStore, IBootstrapConfig config, IErrorHandler errorHandler)
    {

#if DEBUG
        Console.WriteLine("JAP Server started...");
#endif        
        var sw = Stopwatch.StartNew();

        var getSession = sessionStore.GetSession(sessionKey);
        if (!getSession.Succeeded)
        {
            errorHandler.Handle(getSession.ToResult(), sessionGuid: sessionKey);
            return 1;
        }

        var jobName    = getSession.Value.JobTypeName;
        var jobExePath = Path.Combine(config.BackendRoot, "JobRepository", jobName, jobName + ".exe");

        var japServer = new JAPServer(sessionStore, sessionKey, jobName, errorHandler, configStore);
        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(japServer);
        if (!dispatcherResult.Succeeded)
        {
            var err = dispatcherResult.ToResult();
            errorHandler.Handle(err, jobName: jobName, sessionGuid: sessionKey);
            return 1;
        }
        var jipDispatcher = dispatcherResult.Value;

        var serverStartArguments = new ServerStartArguments
        {
            ClientExePath = jobExePath,
            ConnectionTimeout = TimeSpan.FromMinutes(1),
            HandleStringRequest = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey = sessionKey,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, sessionKey, errorHandler),
        };

#if DEBUG
        Console.WriteLine("Invoking PipesServer.RunAsync()...");
#endif
        SetStatus(sessionStore, sessionKey, ExecutionStatus.Running, errorHandler, jobName);

        var res = await PipesServer.RunAsync(serverStartArguments);

#if DEBUG
        Console.WriteLine($"Finished after {sw.Elapsed}");
#endif
        if (!res.Succeeded)
        {
            SetStatus(sessionStore, sessionKey, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(res, jobName: jobName, sessionGuid: sessionKey);
            return 1;
        }

        if (!japServer.ErrorWasReported)
            SetStatus(sessionStore, sessionKey, ExecutionStatus.FinishedSuccessfully, errorHandler, jobName);

        LocalLog.WriteInfo("Server terminiert.");
        return 0;
    }

    // -------------------------------------------------------------------------
    // Dispatch
    // -------------------------------------------------------------------------

    private static async Task<string> HandleRequest(IJipDispatcher dispatcher, string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED> {requestStr}");
        var responseStr = await dispatcher.Dispatch(requestStr);
        Console.WriteLine($"SRV|SENDING>  {responseStr}");
        return responseStr;
    }

    private static async Task HandleHandlerError(
        string request, Exception ex, string jobName, Guid sessionGuid, IErrorHandler errorHandler)
    {
        var msg = $"Fehler beim Verarbeiten der Anfrage: {request}";
        errorHandler.Handle(msg, callStack: null, exceptionDetails: ex.ToString(), jobName: jobName, sessionGuid: sessionGuid);
        await Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // Status helpers
    // -------------------------------------------------------------------------

    private static void SetStatus(
        SessionStore sessionStore, Guid sessionGuid, ExecutionStatus status,
        IErrorHandler errorHandler, string? jobName = null)
    {
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with { ExecutionStatus = status };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }
}
