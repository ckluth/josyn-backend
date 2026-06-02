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
        try
        {
            Console.WriteLine("ARGS: " + string.Join(" | ", args));
            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
            {
                LocalLog.WriteError("Keine IPC-Session-UID angegeben.");
                return 1;
            }

            return await RunServer(sessionKey);
        }
        catch (Exception ex)
        {
            LocalLog.WriteError("Unbehandelte Exception im Host.", exceptionDetails: ex.ToString());
            Console.WriteLine(ex);
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

    private static async Task<int> RunServer(Guid sessionKey)
    {
        Console.WriteLine("Starting Server...");
        var sw = Stopwatch.StartNew();

        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(_japServer);
        if (!dispatcherResult.Succeeded)
        {
            LocalLog.WriteError(dispatcherResult.ToResult());
            return 1;
        }
        var jipDispatcher = dispatcherResult.Value;

        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout    = TimeSpan.FromDays(1),
            HandleStringRequest  = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey           = sessionKey,
            HandleErrorNotification = HandleHandlerError,
            IsCancellationRequested = WasEscapePressed,
        };

        var res = await PipesServer.RunAsync(serverStartArguments, true, () =>
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nResestablishing Connection\n");
            Console.ResetColor();
        });

        Console.WriteLine($"Finished after {sw.Elapsed}");
        if (!res.Succeeded)
        {
            LocalLog.WriteError(res);
            return 1;
        }

        LocalLog.WriteInfo("Server terminiert.");
        return 0;
    }

    private static Task<bool> WasEscapePressed()
    {
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return Task.FromResult(false);
        Console.WriteLine("ESC gedrückt. Abbruch...");
        return Task.FromResult(true);
    }

    // -------------------------------------------------------------------------
    // Dispatch
    // -------------------------------------------------------------------------

    private static readonly JAPServer _japServer = new();

    private static async Task<string> HandleRequest(IJipDispatcher dispatcher, string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED> {requestStr}");
        var responseStr = await dispatcher.Dispatch(requestStr);
        Console.WriteLine($"SRV|SENDING>  {responseStr}");
        return responseStr;
    }

    private static async Task HandleHandlerError(string request, Exception ex)
    {
        LocalLog.WriteError($"Fehler beim Verarbeiten der Anfrage: {request}", exceptionDetails: ex.ToString());
        await Task.CompletedTask;
    }
}
