using System.Text.Json;
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
    private readonly TaskCompletionSource<bool> _negotiationGate =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Completes when <see cref="AcceptSession"/> or <see cref="RejectSession"/> is called.
    /// <c>true</c> = accepted; <c>false</c> = rejected.
    /// Awaited by Host with a timeout to enforce the 30-second negotiation window.
    /// </summary>
    internal Task<bool> NegotiationOutcome => _negotiationGate.Task;

    /// <summary>True if a terminal status was already set by a protocol call.</summary>
    internal bool TerminalStatusSet { get; private set; }

    // -------------------------------------------------------------------------
    // Session start negotiation
    // -------------------------------------------------------------------------

    Task<Result> IJosynApplicationProtocol.AcceptSession()
    {
        SetStatus(ExecutionStatus.Running);
        _negotiationGate.TrySetResult(true);
        return Task.FromResult(Result.Success);
    }

    Task<Result> IJosynApplicationProtocol.RejectSession()
    {
        SetTerminalStatus(ExecutionStatus.FinishedRejected);
        _negotiationGate.TrySetResult(false);
        return Task.FromResult(Result.Success);
    }

    Task<Result<string>> IJosynApplicationProtocol.GetConcurrentSessionArguments()
    {
        var get = sessionStore.GetConcurrentSessionArguments(sessionGuid, jobName);
        if (!get.Succeeded)
            return Task.FromResult(get.ToResult<string>());
        var json = JsonSerializer.Serialize(get.Value.ToList());
        return Task.FromResult(Result<string>.Success(json));
    }

    // -------------------------------------------------------------------------
    // Job execution
    // -------------------------------------------------------------------------

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

        var updated = (JobSessionRecord)get.Value with { Result = result };
        var save = sessionStore.UpdateSession(updated);
        return Task.FromResult(!save.Succeeded ? Result.Propagate(save) : Result.Success);
    }

    Task<Result> IJosynApplicationProtocol.PutDomainError(string? description)
    {
        SetTerminalStatus(ExecutionStatus.FinishedWithErrors, result: description);
        return Task.FromResult(Result.Success);
    }

    Task<Result> IJosynApplicationProtocol.PutError(string serializedError)
    {
        SetTerminalStatus(ExecutionStatus.FinishedFaulted);

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
    // Helpers
    // -------------------------------------------------------------------------

    private void SetStatus(ExecutionStatus status)
    {
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with { ExecutionStatus = status };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }

    private void SetTerminalStatus(ExecutionStatus status, string? result = null)
    {
        TerminalStatusSet = true;
        var get = sessionStore.GetSession(sessionGuid);
        if (!get.Succeeded) { errorHandler.Handle(get.ToResult(), jobName: jobName, sessionGuid: sessionGuid); return; }
        var updated = (JobSessionRecord)get.Value with
        {
            ExecutionStatus = status,
            Finished        = DateTime.Now,
            Result          = result ?? get.Value.Result
        };
        var save = sessionStore.UpdateSession(updated);
        if (!save.Succeeded) errorHandler.Handle(save, jobName: jobName, sessionGuid: sessionGuid);
    }
}

