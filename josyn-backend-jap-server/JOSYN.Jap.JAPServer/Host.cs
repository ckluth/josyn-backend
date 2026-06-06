using JOSYN.Backend.AdapterContracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Shared.Contract;
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
            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
            {
                const string msg = "Keine IPC-Session-UID angegeben.";
                LocalLog.WriteError(msg);
                errorHandler.Handle(msg, null, null);
                return 1;
            }

            var sessionStore = new SessionStore(config.SessionStoreConnectionString);

            var getSource = ResolveConfigSource(config);
            if (!getSource.Succeeded)
            {
                var err = getSource.ToResult();
                LocalLog.WriteError(err);
                errorHandler.Handle(err, sessionGuid: null);
                return 1;
            }
            var configStore = new ConfigStore(getSource.Value, config.SessionStoreConnectionString);

            return await RunServer(sessionKey, sessionStore, configStore, config, errorHandler);
        }
        catch (Exception ex)
        {
            string msg = "Unbehandelte Exception im Host.";
            LocalLog.WriteError(msg, exceptionDetails: ex.ToString());
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
            var msg = $"Session nicht gefunden: {sessionKey}";
            LocalLog.WriteError(msg);
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
            LocalLog.WriteError(err);
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
        var res = await PipesServer.RunAsync(serverStartArguments);

#if DEBUG
        Console.WriteLine($"Finished after {sw.Elapsed}");
#endif                        
        if (!res.Succeeded)
        {
            LocalLog.WriteError(res);
            errorHandler.Handle(res, jobName: jobName, sessionGuid: sessionKey);
            return 1;
        }

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
        LocalLog.WriteError(msg, exceptionDetails: ex.ToString());
        errorHandler.Handle(msg, callStack: null, exceptionDetails: ex.ToString(), jobName: jobName, sessionGuid: sessionGuid);
        await Task.CompletedTask;
    }
}
