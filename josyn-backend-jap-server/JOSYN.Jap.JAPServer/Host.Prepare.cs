using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.Contracts;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Adapter.IdentityAdapter.Contract;
using JOSYN.Backend.SessionStore;
using JOSYN.Commons.Helpers;
using JOSYN.Commons.IdentityHelpers;
using JOSYN.Commons.Log;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using System.Diagnostics;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    //
    // PrepareContext — immutable snapshot of the Prepare phase outputs.
    // Constructed once at the end of Prepare; never mutated after construction.
    //
    private sealed record PrepareContext(
        Guid          SessionGuid,
        JAPServer?    JapServer,
        Task<Result>? ServerTask,
        bool          NegotiationAccepted,
        Result        InnerError);

    // Intermediate result of BuildJapInfrastructure — groups the two objects
    // that are constructed together and both needed by subsequent phases.
    private sealed record JapInfrastructure(JAPServer JapServer, IJipDispatcher JipDispatcher);

    // Outcome of the negotiation phase — either the running server task (accepted)
    // or null (timeout / rejected, server already awaited and torn down).
    private sealed record NegotiationOutcome(Task<Result>? ServerTask, bool Accepted);

    //
    // Executes inside the Turnstile: allocates the session GUID, persists the session record,
    // starts the pipe server and awaits the accept/reject negotiation outcome.
    //
    // Return type is Task<PrepareContext>, not Task<Result<PrepareContext>>.
    // This is intentional: Turnstile.RunAsync<T> already wraps the return value in Result<T>,
    // so the outer Result layer is provided by the Turnstile (covers timeout and unhandled exceptions).
    // Internal failures — those that are anticipated and handled — are recorded in ctx.InnerError,
    // which the caller inspects after the Turnstile returns successfully.
    // Each phase returns its output via Result<T>; Prepare threads these as inputs to the next
    // phase and constructs an immutable PrepareContext at the very end.
    //
    private static async Task<PrepareContext> Prepare(
        SessionStore     sessionStore,
        string           jobName,
        string           jobExePath,
        IBootstrapConfig bootStrapConfig,
        SessionStartSpec startSpec,
        string           plainArguments,
        IErrorHandler    errorHandler,
        AdapterManager   adapterManager)
    {
        var createSession = CreateSessionRecord(sessionStore, jobName, jobExePath, startSpec, plainArguments);
        if (!createSession.Succeeded)
            return new PrepareContext(Guid.Empty, null, null, false, createSession.ToResult());

        var sessionGuid = createSession.Value;

        var buildInfra = BuildJapInfrastructure(sessionGuid, sessionStore, jobName, bootStrapConfig, errorHandler, adapterManager);
        if (!buildInfra.Succeeded)
            return new PrepareContext(sessionGuid, null, null, false, buildInfra.ToResult());

        var (japServer, jipDispatcher) = buildInfra.Value;

        var credentials = await ResolveCredentials(sessionGuid, sessionStore, jobName, startSpec.TechnicalUserName, adapterManager, errorHandler);
        if (!credentials.Succeeded)
            return new PrepareContext(sessionGuid, japServer, null, false, credentials.ToResult());

        var launch = LaunchJobAndStorePid(sessionGuid, startSpec.TechnicalUserName, credentials.Value, sessionStore, jobExePath, jobName, errorHandler, startSpec.Interactive);
        if (!launch.Succeeded)
            return new PrepareContext(sessionGuid, japServer, null, false, launch);

        var negotiation = await RunNegotiation(sessionGuid, japServer, jipDispatcher, sessionStore, jobName, errorHandler);
        return new PrepareContext(sessionGuid, japServer, negotiation.ServerTask, negotiation.Accepted, Result.Success);
    }

    //
    // Allocates a new session GUID and persists a new JobSessionRecord with the initial info about the session.
    //
    private static Result<Guid> CreateSessionRecord(
        SessionStore     sessionStore,
        string           jobName,
        string           jobExePath,
        SessionStartSpec startSpec,
        string           plainArguments)
    {
        var getJobExecutableVersion = GetJobExecutableVersion(jobExePath);
        if (!getJobExecutableVersion.Succeeded)
            return Result<Guid>.Propagate(getJobExecutableVersion.ToResult<Guid>());

        var sessionGuid = Guid.NewGuid();
        var save = sessionStore.SaveNewSession(new JobSessionRecord
        {
            UID                = sessionGuid,
            JobTypeName        = jobName,
            Arguments          = plainArguments,
            Result             = string.Empty,
            JobVersion         = getJobExecutableVersion.Value,
            UserName           = startSpec.CallerUser,
            UserDomain         = startSpec.CallerDomain,
            ClientApplication  = startSpec.CallerApplication,
            ClientMachine      = startSpec.CallerMachine,
            TecUser            = startSpec.TechnicalUserName,
            Started            = DateTime.Now,
            ExecutionStatus    = ExecutionStatus.Preparing,
            JapServerProcessId = Environment.ProcessId,
            JobHostProcessId   = 0,
            Host               = Environment.MachineName,
        });

        if (save.Succeeded) return sessionGuid;

        // Record was never persisted — no valid GUID to reference.
        return Result<Guid>.Propagate(save.ToResult<Guid>());

        
        // ===================================================================
        //
        // nested functions
        //
        
        static Result<string> GetJobExecutableVersion(string jobExePath)
        {
            if (!File.Exists(jobExePath))
                return Result.Error($"Job executable not found: '{jobExePath}'");

            var jobVersion = string.Empty;
            try
            {
                jobVersion = FileVersionInfo.GetVersionInfo(jobExePath).ProductVersion ?? string.Empty;
            }
            catch { /* no version */ }

            if (string.IsNullOrEmpty(jobVersion))
                return Result.Error($"Job executable has no version information: '{jobExePath}'");

            return jobVersion;
        }
    }

    //
    // Resolves the configuration source and sets up the JAPServer and JIP dispatcher.
    //
    private static Result<JapInfrastructure> BuildJapInfrastructure(
        Guid             sessionGuid,
        SessionStore     sessionStore,
        string           jobName,
        IBootstrapConfig bootStrapConfig,
        IErrorHandler    errorHandler,
        AdapterManager   adapterManager)
    {
        var configStore = new ConfigStore(bootStrapConfig.SessionStoreConnectionString);

        // Set up JAPServer with session info and register all JAP protocol handlers on the JIP dispatcher.
        var japServer        = new JAPServer(sessionStore, sessionGuid, jobName, errorHandler, configStore, adapterManager);
        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(japServer);
        if (!dispatcherResult.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result<JapInfrastructure>.Propagate(dispatcherResult.ToResult<JapInfrastructure>());
        }

        return new JapInfrastructure(japServer, dispatcherResult.Value);
    }

    //
    // Calls the IdentityAdapter to resolve the password for the TechnicalUserName.
    //
    private static async Task<Result<string>> ResolveCredentials(
        Guid           sessionGuid,
        SessionStore   sessionStore,
        string         jobName,
        string         technicalUserName,
        AdapterManager adapterManager,
        IErrorHandler  errorHandler)
    {
        var getPipes = adapterManager.GetPipes(IdentityAdapterConcern);
        if (!getPipes.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result<string>.Propagate(getPipes.ToResult<string>());
        }

        var getPassword = await JipClient.SendAsync(getPipes.Value, nameof(IIdentityAdapter.GetPassword), technicalUserName);
        if (!getPassword.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result<string>.Propagate(getPassword.ToResult<string>());
        }

        return getPassword.Value;
    }

    private static Result LaunchJobAndStorePid(
        Guid          sessionGuid,
        string        technicalUserName,
        string        technicalUserPassword,
        SessionStore  sessionStore,
        string        jobExePath,
        string        jobName,
        IErrorHandler errorHandler,
        bool          interactive = false)
    {
        if (!OperatingSystem.IsWindows())
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result.Fail("Impersonated process launch is not supported on non-Windows platforms.");
        }

        var parseCredential = WindowsCredential.Parse(technicalUserName);
        if (!parseCredential.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result.Propagate(parseCredential.ToResult());
        }

        var arguments = PipesProtocol.CreateClientStartCLIArguments(sessionGuid.ToString());
        var launch    = ImpersonatedProcess.Start(jobExePath, arguments, technicalUserPassword, parseCredential.Value, headless: !interactive);
        if (!launch.Succeeded)
        {
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result.Propagate(launch.ToResult());
        }

        var getSession = sessionStore.GetSession(sessionGuid);
        if (!getSession.Succeeded)
        {
            KillProcess(launch.Value);
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result.Propagate(getSession.ToResult());
        }

        // Guard: ISessionStore must return a JobSessionRecord. Any other type is a contract violation.
        if (getSession.Value is not JobSessionRecord sessionRecord)
        {
            KillProcess(launch.Value);
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            return Result.Fail("Session record is not a JobSessionRecord.");
        }

        // Persist the job host process ID. Must succeed before the pipe server is started —
        // failure here kills the already-running job.exe and aborts the Prepare phase.
        var storePid = sessionStore.UpdateSession(sessionRecord with { JobHostProcessId = launch.Value });
        if (storePid.Succeeded) return Result.Success;

        KillProcess(launch.Value);
        SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
        return storePid;

        // ── helpers ───────────────────────────────────────────────────────
        static void KillProcess(int pid)
        {
            try { Process.GetProcessById(pid).Kill(); }
            catch { /* process may have already exited — ignore */ }
        }
    }

    //
    // Awaits the negotiation outcome from JAPServer.NegotiationOutcome, which is set by the
    // AcceptSession / RejectSession handlers. On timeout or rejection the pipe server is
    // already awaited and torn down before this method returns; ServerTask is null in that case.
    //
    private static async Task<NegotiationOutcome> RunNegotiation(
        Guid           sessionGuid,
        JAPServer      japServer,
        IJipDispatcher jipDispatcher,
        SessionStore   sessionStore,
        string         jobName,
        IErrorHandler  errorHandler)
    {
        // Local flag captured by the IsCancellationRequested lambda.
        // C# closures capture by variable reference, so assignments below are seen by the lambda.
        var shouldCancel = false;

        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout       = TimeSpan.FromMinutes(1),
            HandleStringRequest     = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey              = sessionGuid,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, sessionGuid, errorHandler),
            IsCancellationRequested = () => Task.FromResult(shouldCancel),
        };

        var serverTask = PipesServer.RunAsync(serverStartArguments);
        var winner     = await Task.WhenAny(japServer.NegotiationOutcome, Task.Delay(TimeSpan.FromSeconds(30)));

        if (winner != japServer.NegotiationOutcome)
        {
            // Timeout — job.exe never called AcceptSession or RejectSession.
            LocalLog.WriteError($"Negotiation timeout for session '{sessionGuid}' ({jobName}) — treating as rejected.");
            SetTerminalStatus(sessionStore, sessionGuid, ExecutionStatus.FinishedRejected, errorHandler, jobName);
            shouldCancel = true;
            await serverTask;
            return new NegotiationOutcome(null, false);
        }

        if (!japServer.NegotiationOutcome.Result)
        {
            // Rejected — JAPServer.RejectSession already set FinishedRejected.
            shouldCancel = true;
            await serverTask;
            return new NegotiationOutcome(null, false);
        }

        //
        // Accepted — JAPServer.AcceptSession already set Running.
        // Turnstile releases here: session is definitively in-flight.
        //
        return new NegotiationOutcome(serverTask, true);
    }

}
