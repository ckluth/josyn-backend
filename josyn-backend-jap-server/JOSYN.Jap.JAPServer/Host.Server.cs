using JOSYN.Backend.Contracts;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    // -------------------------------------------------------------------------
    // Dispatch
    // -------------------------------------------------------------------------

    private static async Task<string> HandleRequest(IJipDispatcher dispatcher, string requestStr)
    {
        var responseStr = await dispatcher.Dispatch(requestStr);
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
