using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.GlobalConfig;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.JIP;
using JOSYN.Jap.Shared.Contract;
using JOSYN.Jap.Shared.Log;
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
        var config = new HardcodedGlobalConfig();
        var errorHandler = new FileSystemErrorHandler(config);

        try
        {
            Console.WriteLine("ARGS: " + string.Join(" | ", args));
            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
            {
                var msg = "Keine IPC-Session-UID angegeben.";
                LocalLog.WriteError(msg);
                errorHandler.Handle(msg);
                return 1;
            }

            var sessionStore = new SessionStore(config.SessionStoreConnectionString);

            return await RunServer(sessionKey, sessionStore, config, errorHandler);
        }
        catch (Exception ex)
        {
            var msg = "Unbehandelte Exception im Host.";
            LocalLog.WriteError(msg, exceptionDetails: ex.ToString());
            errorHandler.Handle(msg, ex);
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
    // Server lifecycle
    // -------------------------------------------------------------------------

    private static async Task<int> RunServer(
        Guid sessionKey, SessionStore sessionStore, IGlobalConfig config, IErrorHandler errorHandler)
    {
        Console.WriteLine("Starting Server...");
        var sw = Stopwatch.StartNew();

        var getSession = sessionStore.GetSession(sessionKey);
        if (!getSession.Succeeded)
        {
            var msg = $"Session nicht gefunden: {sessionKey}";
            LocalLog.WriteError(msg);
            errorHandler.Handle(msg);
            return 1;
        }

        var jobExePath = Path.Combine(config.JobRepositoryRoot, getSession.Value.JobTypeName + ".exe");

        var japServer        = new JAPServer(sessionStore, sessionKey);
        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(japServer);
        if (!dispatcherResult.Succeeded)
        {
            LocalLog.WriteError(dispatcherResult.ToResult());
            errorHandler.Handle(dispatcherResult.ToResult().ErrorMessage ?? "JipDispatcher-Fehler.");
            return 1;
        }
        var jipDispatcher = dispatcherResult.Value;

        var serverStartArguments = new ServerStartArguments
        {
            ClientExePath        = jobExePath,
            ConnectionTimeout    = TimeSpan.FromMinutes(1),
            HandleStringRequest  = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey           = sessionKey,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, errorHandler),
        };

        var res = await PipesServer.RunAsync(serverStartArguments);

        Console.WriteLine($"Finished after {sw.Elapsed}");
        if (!res.Succeeded)
        {
            LocalLog.WriteError(res);
            errorHandler.Handle(res.ErrorMessage ?? "PipesServer-Fehler.");
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

    private static async Task HandleHandlerError(string request, Exception ex, IErrorHandler errorHandler)
    {
        var msg = $"Fehler beim Verarbeiten der Anfrage: {request}";
        LocalLog.WriteError(msg, exceptionDetails: ex.ToString());
        errorHandler.Handle(msg, ex);
        await Task.CompletedTask;
    }
}
