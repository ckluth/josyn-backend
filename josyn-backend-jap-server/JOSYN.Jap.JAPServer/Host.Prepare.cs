using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.Contracts;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.IdentityAdapter.Contract;
using JOSYN.Backend.SessionStore;
using JOSYN.Commons.Log;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using System.Diagnostics;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    //
    // PrepareContext — carries mutable outputs of the Prepare phase
    //
    private sealed class PrepareContext
    {
        public Guid SessionGuid = Guid.Empty;
        public bool ShouldCancelServer;
        public Task<Result>? ServerTask;
        public JAPServer? JapServer;
        public IJipDispatcher? JipDispatcher;
        public bool NegotiationAccepted;
        public Result InnerError = Result.Success;
        public string? TechnicalUserPassword;
    }

    //
    // Executes inside the Turnstile: allocates the session GUID, persists the session record,
    // starts the pipe server and awaits the accept/reject negotiation outcome.
    //
    // Return type is Task<PrepareContext>, not Task<Result<PrepareContext>>.
    // This is intentional: Turnstile.RunAsync<T> already wraps the return value in Result<T>,
    // so the outer Result layer is provided by the Turnstile (covers timeout and unhandled exceptions).
    // Internal failures — those that are anticipated and handled — are recorded in ctx.InnerError,
    // which the caller inspects after the Turnstile returns successfully.
    // A phase that fails sets ctx.InnerError and returns false; subsequent phases are skipped.
    // The ctx is always returned, even on failure, so the caller always has access to partial state
    // (e.g. ctx.SessionGuid for error logging).
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
        var ctx = new PrepareContext();
        if (!CreateSessionRecord(ctx, sessionStore, jobName, jobExePath, startSpec, plainArguments)
            || !BuildJapInfrastructure(ctx, sessionStore, jobName, bootStrapConfig, errorHandler, adapterManager)
            || !await ResolveCredentials(ctx, sessionStore, jobName, startSpec.TechnicalUserName, adapterManager, errorHandler)
            || !LaunchJobAndStorePid(ctx, sessionStore, jobExePath, jobName, errorHandler)) return ctx;
        await RunNegotiation(ctx, sessionStore, jobName, errorHandler);
        return ctx;
    }

    //
    // Allocates a new session GUID and persists a new JobSessionRecord with the initial info about the session.
    //
    private static bool CreateSessionRecord(
        PrepareContext ctx,
        SessionStore sessionStore,
        string jobName,
        string jobExePath,
        SessionStartSpec startSpec,
        string plainArguments)
    {
        var getJobExecutableVersion = GetJobExecutableVersion(jobExePath);
        if (!getJobExecutableVersion.Succeeded)
        {
            ctx.InnerError = getJobExecutableVersion.ToResult();
            return false;
        }

        ctx.SessionGuid = Guid.NewGuid();
        var save = sessionStore.SaveNewSession(new JobSessionRecord
        {
            UID = ctx.SessionGuid,
            JobTypeName = jobName,
            Arguments = plainArguments,
            Result = string.Empty,
            JobVersion = getJobExecutableVersion.Value,
            UserName = startSpec.CallerUser,
            UserDomain = startSpec.CallerDomain,
            ClientApplication = startSpec.CallerApplication,
            ClientMachine = startSpec.CallerMachine,
            TecUser = startSpec.TechnicalUserName,
            Started = DateTime.Now,
            ExecutionStatus = ExecutionStatus.Preparing,
            JapServerProcessId = Environment.ProcessId,
            JobHostProcessId = 0,
            Host = Environment.MachineName,
        });

        if (save.Succeeded) return true;

        ctx.SessionGuid = Guid.Empty; // record was never persisted — no valid session to reference
        ctx.InnerError = save;
        return false;
        
        //
        // nested funtion
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
    private static bool BuildJapInfrastructure(
        PrepareContext ctx,
        SessionStore sessionStore,
        string jobName,
        IBootstrapConfig bootStrapConfig,
        IErrorHandler errorHandler,
        AdapterManager adapterManager)
    {
        var configStore = new ConfigStore(bootStrapConfig.SessionStoreConnectionString);

        // Set up JAPServer with session info and register all JAP protocol handlers on the JIP dispatcher.
        ctx.JapServer = new JAPServer(sessionStore, ctx.SessionGuid, jobName, errorHandler, configStore, adapterManager);
        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(ctx.JapServer);
        if (!dispatcherResult.Succeeded)
        {
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = dispatcherResult.ToResult();
            return false;
        }

        ctx.JipDispatcher = dispatcherResult.Value;
        return true;
    }

    //
    // Calls the IdentityAdapter to resolve the password for the TechnicalUserName.
    // The password is stored in ctx for use during job.exe spawn.
    //
    private static async Task<bool> ResolveCredentials(
        PrepareContext ctx,
        SessionStore   sessionStore,
        string         jobName,
        string         technicalUserName,
        AdapterManager adapterManager,
        IErrorHandler  errorHandler)
    {
        var getPipes = adapterManager.GetPipes(IdentityAdapterConcern);
        if (!getPipes.Succeeded)
        {
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = getPipes.ToResult();
            return false;
        }

        var getPassword = await JipClient.SendAsync(getPipes.Value, nameof(IIdentityAdapter.GetPassword), technicalUserName);
        if (!getPassword.Succeeded)
        {
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = getPassword.ToResult();
            return false;
        }

        ctx.TechnicalUserPassword = getPassword.Value;
        return true;
    }


    private static bool LaunchJobAndStorePid(
        PrepareContext ctx,
        SessionStore sessionStore,
        string jobExePath,
        string jobName,
        IErrorHandler errorHandler)
    {
        // TODO (ADR-017B-03): replace with an impersonated Process.Start using
        // ctx.TechnicalUserPassword and TechnicalUserName from the session record.
        // ctx.TechnicalUserPassword is already resolved at this point.
        var launch = PipesServer.StartClientProcess(jobExePath, ctx.SessionGuid);
        if (!launch.Succeeded)
        {
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = launch.ToResult();
            return false;
        }

        var getSession = sessionStore.GetSession(ctx.SessionGuid);
        if (!getSession.Succeeded)
        {
            KillProcess(launch.Value);
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = getSession.ToResult();
            return false;
        }

        // Guard: ISessionStore must return a JobSessionRecord. Any other type is a contract violation.
        if (getSession.Value is not JobSessionRecord sessionRecord)
        {
            KillProcess(launch.Value);
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = Result.Fail("Session record is not a JobSessionRecord.");
            return false;
        }

        // Persist the job host process ID. Must succeed before the pipe server is started —
        // failure here kills the already-running job.exe and aborts the Prepare phase.
        var storePid = sessionStore.UpdateSession(sessionRecord with { JobHostProcessId = launch.Value });

        if (storePid.Succeeded) return true;

        KillProcess(launch.Value);
        SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
        ctx.InnerError = storePid;
        return false;
        
        //
        // Nested function...
        //        
        static void KillProcess(int pid)
        {
            try { Process.GetProcessById(pid).Kill(); }
            catch { /* process may have already exited — ignore */ }
        }        
    }

    //
    // Awaits the negotiation outcome from JAPServer.NegotiationOutcome, which is set by the AcceptSession/RejectSession handlers.
    //
    private static async Task RunNegotiation(
        PrepareContext ctx,
        SessionStore sessionStore,
        string jobName,
        IErrorHandler errorHandler)
    {
        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout = TimeSpan.FromMinutes(1),
            HandleStringRequest = requestStr => HandleRequest(ctx.JipDispatcher!, requestStr),
            SessionKey = ctx.SessionGuid,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, ctx.SessionGuid, errorHandler),
            IsCancellationRequested = () => Task.FromResult(ctx.ShouldCancelServer),
        };

        ctx.ServerTask = PipesServer.RunAsync(serverStartArguments);

        var winner = await Task.WhenAny(ctx.JapServer!.NegotiationOutcome, Task.Delay(TimeSpan.FromSeconds(30)));

        if (winner != ctx.JapServer.NegotiationOutcome)
        {
            // Timeout — job.exe never called AcceptSession or RejectSession.
            LocalLog.WriteError($"Negotiation timeout for session '{ctx.SessionGuid}' ({jobName}) — treating as rejected.");
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedRejected, errorHandler, jobName);
            ctx.ShouldCancelServer = true;
            await ctx.ServerTask;
            ctx.ServerTask = null;
            return;
        }

        if (!ctx.JapServer.NegotiationOutcome.Result)
        {
            // Rejected — JAPServer.RejectSession already set FinishedRejected.
            ctx.ShouldCancelServer = true;
            await ctx.ServerTask;
            ctx.ServerTask = null;
            return;
        }

        //
        // Accepted — JAPServer.AcceptSession already set Running.
        // Turnstile releases here: session is definitively in-flight.
        //
        ctx.NegotiationAccepted = true;
    }

}
