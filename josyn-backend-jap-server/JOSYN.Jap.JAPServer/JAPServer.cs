using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Shared.Contract;
using JOSYN.Commons.Log;

namespace JOSYN.Jap.JAPServer;

internal sealed class JAPServer(
    ISessionStore  sessionStore,
    Guid           sessionGuid,
    string         jobName,
    IErrorHandler  errorHandler) : IJosynApplicationProtocol
{
    Task<Result<string>> IJosynApplicationProtocol.GetRawArguments()
    {
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded)
            return Task.FromResult<Result<string>>(get.ToResult<string>());
        return Task.FromResult<Result<string>>(get.Value.Arguments);
    }

    Task<Result> IJosynApplicationProtocol.PutRawResult(string result)
    {
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded)
            return Task.FromResult(Result.Propagate(get.ToResult()));

        var session = get.Value;
        var updated = new JobSessionRecord
        {
            UID         = session.UID,
            JobTypeName = session.JobTypeName,
            Arguments   = session.Arguments,
            Result      = result
        };

        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded)
            return Task.FromResult(Result.Propagate(save));

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[PROCESSING]");
        Console.WriteLine(result);
        Console.ResetColor();

        return Task.FromResult(Result.Success);
    }

    Task<Result> IJosynApplicationProtocol.PutError(string serializedError)
    {
        var deserialize = PropertyBag.Deserialize<ErrorReport>(serializedError);
        if (!deserialize.Succeeded)
        {
            LocalLog.WriteError($"ErrorReport konnte nicht deserialisiert werden: {deserialize.ErrorMessage}\nRaw: {serializedError}");
            return Task.FromResult(Result.Propagate(deserialize.ToResult()));
        }
        var report = deserialize.Value;
        LocalLog.WriteError(report.Causer, report.Message, report.CallStack, report.ExceptionDetails);
        errorHandler.Handle(
            report.Message,
            report.CallStack,
            report.ExceptionDetails,
            jobName,
            sessionGuid);
        return Task.FromResult(Result.Success);
    }
}
