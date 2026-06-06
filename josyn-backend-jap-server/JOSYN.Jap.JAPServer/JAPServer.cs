using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Shared.Contract;

namespace JOSYN.Jap.JAPServer;

internal sealed class JAPServer(
    ISessionStore  sessionStore,
    Guid           sessionGuid,
    string         jobName,
    IErrorHandler  errorHandler,
    IConfigStore   configStore) : IJosynApplicationProtocol
{
    Task<Result<string>> IJosynApplicationProtocol.GetRawArguments()
    {
        var get = sessionStore.GetSession(sessionGuid);
        return !get.Succeeded 
            ? Task.FromResult<Result<string>>(get.ToResult<string>()) 
            : Task.FromResult<Result<string>>(get.Value.Arguments);
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
            errorHandler.Handle(deserialize.ToResult(), jobName: jobName, sessionGuid: sessionGuid);
            return Task.FromResult(Result.Propagate(deserialize.ToResult()));
        }
        var report = deserialize.Value;
        errorHandler.Handle(
            report.Message,
            report.CallStack,
            report.ExceptionDetails,
            jobName,
            sessionGuid);
        return Task.FromResult(Result.Success);
    }

    Task<Result<RuntimeEnvironment>> IJosynApplicationProtocol.GetEnvironment()
    {
        var get = configStore.GetValue(ConfigKeys.RuntimeEnvironment);
        if (!get.Succeeded)
            return Task.FromResult<Result<RuntimeEnvironment>>(get.ToResult<RuntimeEnvironment>());
     
        if (!Enum.TryParse<RuntimeEnvironment>(get.Value, out var env))
            return Task.FromResult<Result<RuntimeEnvironment>>(
                Result<RuntimeEnvironment>.Fail($"Ungültiger RuntimeEnvironment-Wert in ConfigStore: '{get.Value}'"));
        
        return Task.FromResult<Result<RuntimeEnvironment>>(env);
    }
}
