using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;

namespace JOSYN.Jap.JAPServer;

internal sealed class JAPServer(
    ISessionStore  sessionStore,
    Guid           sessionGuid,
    string         jobName,
    IErrorHandler  errorHandler,
    IConfigStore   configStore) : IJosynApplicationProtocol
{
    private bool _errorReported;

    /// <summary>True if <see cref="IJosynApplicationProtocol.PutError"/> was called during this session.</summary>
    internal bool ErrorWasReported => _errorReported;
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
            UID               = session.UID,
            JobTypeName       = session.JobTypeName,
            Arguments         = session.Arguments,
            Result            = result,
            JobVersion        = session.JobVersion,
            UserName          = session.UserName,
            UserDomain        = session.UserDomain,
            ClientApplication = session.ClientApplication,
            ClientMachine     = session.ClientMachine,
            TecUser           = session.TecUser,
            Started           = session.Started,
            ExecutionStatus   = session.ExecutionStatus,
            Progress          = session.Progress,
            Finished          = session.Finished,
            JapServerProcess  = session.JapServerProcess,
            JobHostProcessId  = session.JobHostProcessId,
            JapExitCode       = session.JapExitCode,
            JobExitCode       = session.JobExitCode,
            LastWriteTime     = session.LastWriteTime,
            WrittenBy         = session.WrittenBy
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
        _errorReported = true;
        SetStatus(ExecutionStatus.FinishedFaulted);

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

    // -------------------------------------------------------------------------

    private void SetStatus(ExecutionStatus status)
    {
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with { ExecutionStatus = status };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }
}
