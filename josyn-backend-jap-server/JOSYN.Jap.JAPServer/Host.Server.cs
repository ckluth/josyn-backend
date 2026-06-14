using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.SessionStore;
using JOSYN.Commons.Log;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using System.Diagnostics;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    // -------------------------------------------------------------------------
    // JOSYN-IPC mode (legacy — no negotiation)
    // -------------------------------------------------------------------------

    private static async Task<int> RunServer(
        Guid sessionKey, SessionStore sessionStore, ConfigStore configStore,
        IBootstrapConfig config, IErrorHandler errorHandler)
    {
#if DEBUG
        Console.WriteLine("JAP Server started...");
#endif
        var getSession = sessionStore.GetSession(sessionKey);
        if (!getSession.Succeeded)
        {
            errorHandler.Handle(getSession.ToResult(), sessionGuid: sessionKey);
            return 1;
        }

        var jobName    = getSession.Value.JobTypeName;
        var jobExePath = Path.Combine(config.BackendRoot, JobRepositoryFolder, jobName, jobName + ".exe");

        // Set Preparing + JobVersion — skip negotiation in legacy IPC mode.
        SetPreparingWithVersion(sessionStore, sessionKey, jobExePath, errorHandler, jobName);

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
            ClientExePath           = jobExePath,
            ConnectionTimeout       = TimeSpan.FromMinutes(1),
            HandleStringRequest     = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey              = sessionKey,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, sessionKey, errorHandler),
        };

        // Legacy mode: skip negotiation, transition directly to Running.
        SetStatus(sessionStore, sessionKey, ExecutionStatus.Running, errorHandler, jobName);

#if DEBUG
        Console.WriteLine("Invoking PipesServer.RunAsync()...");
#endif
        var res = await PipesServer.RunAsync(serverStartArguments);

        if (!res.Succeeded)
        {
            if (!japServer.TerminalStatusSet)
                SetTerminalStatus(sessionStore, sessionKey, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(res, jobName: jobName, sessionGuid: sessionKey);
            return 1;
        }

        if (!japServer.TerminalStatusSet)
            SetTerminalStatus(sessionStore, sessionKey, ExecutionStatus.FinishedSuccessfully, errorHandler, jobName);

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

    private static void SetTerminalStatus(
        SessionStore sessionStore, Guid sessionGuid, ExecutionStatus status,
        IErrorHandler errorHandler, string? jobName = null)
    {
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with
        {
            ExecutionStatus = status,
            Finished        = DateTime.Now
        };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }
}
