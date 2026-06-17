using System.Text.Json;
using JOSYN.Backend.ConfigurationAdapter.Contract;
using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;

namespace JOSYN.Jap.JAPServer;

internal sealed class JAPServer(
    ISessionStore  sessionStore,
    Guid           sessionGuid,
    string         jobName,
    IErrorHandler  errorHandler,
    IConfigStore   configStore,
    AdapterManager adapterManager) : IJosynApplicationProtocol
{
    private readonly TaskCompletionSource<bool> negotiationGate = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task<bool> NegotiationOutcome => negotiationGate.Task;

    //
    // TerminalStatusSet: True if a terminal status was already set by a protocol call.
    // Guards against a double-write: the job can report its own final status via a JAP
    // protocol message (handled inside the server task), while the entrypoint also writes
    // a fallback terminal status after the task completes. Without this flag the entrypoint
    // would overwrite the status that the protocol handler already persisted.
    //
    internal bool TerminalStatusSet { get; private set; }

    // -------------------------------------------------------------------------
    // IJosynApplicationProtocol / Session start negotiation
    // -------------------------------------------------------------------------

    Task<Result> IJosynApplicationProtocol.AcceptSession()
    {
        SetStatus(ExecutionStatus.Running);
        negotiationGate.TrySetResult(true);
        return Task.FromResult(Result.Success);
    }

    Task<Result> IJosynApplicationProtocol.RejectSession()
    {
        SetTerminalStatus(ExecutionStatus.FinishedRejected);
        negotiationGate.TrySetResult(false);
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
    // IJosynApplicationProtocol / Job execution
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

    async Task<Result<string>> IJosynApplicationProtocol.GetConfigValue(string settingPath)
    {
        var getPipes = adapterManager.GetPipes(Host.ConfigurationAdapterConcern);
        if (!getPipes.Succeeded)
            return getPipes.ToResult<string>();

        var get = await JipClient.SendAsync(getPipes.Value, nameof(IConfigurationAdapter.GetConfigValue), settingPath);
        if (!get.Succeeded)
            return get.ToResult<string>();

        return get.Value ?? Result<string>.Fail("ConfigurationAdapter returned no value.");
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

