using JOSYN.Backend.Contracts;
using JOSYN.Backend.ConfigStore;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.SessionStore;
using JOSYN.Commons.Helpers;
using JOSYN.Commons.Log;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using System.Diagnostics;
using System.Text;
#pragma warning disable CA1859

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    private const string JobRepositoryFolder = "JobRepository";
    
    /// <summary>
    /// The JAP-Server-Entrypoint fpr running a new Job-Session...
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static async Task<int> Run(string[] args)
    {
        var bootStrapConfig = LoadBootstrapConfig();
        if (bootStrapConfig == null) return 1;
        
        var errorHandler = new SqlErrorHandler(bootStrapConfig.SessionStoreConnectionString);
        // ReSharper disable once InvertIf
        if (args.Length < 2 || args[0] != JapServerConstants.CliModeStart)
        {
            const string err = "Unbekannter oder fehlender Start-Modus. Erwartet: JOSYN-START @<path>";
            errorHandler.Handle(err, null, null);
            return 1;
        }
        
        var sessionStartSpecFilepath = args[1];
        return await ProcessSessionStart(sessionStartSpecFilepath, bootStrapConfig, errorHandler);
    }

    /// <summary>
    /// Verarbeitet den JOSYN-START-Modus: Liest die SessionStartSpec, führt die Turnstile-Logik
    /// für die Session-Akzeptanz durch und wartet nach dem Turnstile auf den JAP-Serve-Loop.
    /// </summary>
    private static async Task<int> ProcessSessionStart(string sessionStartSpecFilepath, IBootstrapConfig bootStrapConfig, IErrorHandler errorHandler)
    {
        var getSpec = GetSessionStartSpec(sessionStartSpecFilepath);
        if (!getSpec.Succeeded) { errorHandler.Handle(getSpec.ToResult()); return 1; }
        var startSpec = getSpec.Value;

        var getPlainArguments = Base64DecodeArgumentsString(startSpec.Arguments);
        if (!getPlainArguments.Succeeded) { errorHandler.Handle(getPlainArguments.ToResult()); return 1; }

        var sessionStore = new SessionStore(bootStrapConfig.SessionStoreConnectionString);
        var jobName      = startSpec.JobTypeName;
        var jobExePath   = Path.Combine(bootStrapConfig.BackendRoot, JobRepositoryFolder, jobName, jobName + ".exe");

        //
        // Turnstile scope: GUID allocation → session persistence → spawn → accept/reject negotiation.
        // Released only when the session is definitively in-flight (running) or closed (finished-rejected).
        //
        var ctx = new PrepareContext();
        var turnstileResult = await Turnstile.RunAsync(jobName, () => Prepare(ctx, sessionStore, jobName, jobExePath, bootStrapConfig, startSpec, getPlainArguments.Value, errorHandler));
        if (!turnstileResult.Succeeded)
        {
            errorHandler.Handle(turnstileResult, sessionGuid: ctx.SessionGuid == Guid.Empty ? null : ctx.SessionGuid);
            return 1;
        }
        
        if (!ctx.InnerError.Succeeded) { errorHandler.Handle(ctx.InnerError, sessionGuid: ctx.SessionGuid); return 1; }
        if (!ctx.NegotiationAccepted)  return 0;
        
        //
        // Session accepted and running — await JAP serve loop.
        // ServerTask is non-null: assigned in PrepareServer and only nulled in the
        // timeout/rejected early-return branches, both of which exit before
        // NegotiationAccepted can be set to true. The guard above ensures we only
        // reach this line via the accepted path.
        //
        var res = await ctx.ServerTask!;
        
        //
        // Finalization
        //
        if (!res.Succeeded)
        {
            if (!ctx.JapServer!.TerminalStatusSet)
                SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            errorHandler.Handle(res, jobName: jobName, sessionGuid: ctx.SessionGuid);
            return 1;
        }

        if (!ctx.JapServer!.TerminalStatusSet)
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedSuccessfully, errorHandler, jobName);

        LocalLog.WriteInfo("Server terminiert.");
        return 0;
    }

    

    // -------------------------------------------------------------------------
    // PrepareContext — carries mutable outputs of the Prepare phase
    // -------------------------------------------------------------------------

    private sealed class PrepareContext
    {
        public Guid SessionGuid = Guid.Empty;
        public bool ShouldCancelServer;
        public Task<Result>? ServerTask;
        public JAPServer? JapServer;
        public bool NegotiationAccepted;
        public Result InnerError = Result.Success;
    }
    
    /// <summary>
    /// Executes inside the Turnstile: allocates the session GUID, persists the session record,
    /// starts the pipe server and awaits the accept/reject negotiation outcome.
    /// Writes all results into <paramref name="ctx"/>; never throws.
    /// </summary>
    private static async Task Prepare(
        PrepareContext   ctx,
        SessionStore     sessionStore,
        string           jobName,
        string           jobExePath,
        IBootstrapConfig bootStrapConfig,
        SessionStartSpec startSpec,
        string           plainArguments,
        IErrorHandler    errorHandler)
    {
        var jobVersion = string.Empty;
        try { jobVersion = FileVersionInfo.GetVersionInfo(jobExePath).ProductVersion ?? string.Empty; }
        catch { /* exe not found or version unreadable — version stays empty */ }

        ctx.SessionGuid = Guid.NewGuid();
        var save = sessionStore.SaveNewSession(new JobSessionRecord
        {
            UID               = ctx.SessionGuid,
            JobTypeName       = jobName,
            Arguments         = plainArguments,
            Result            = string.Empty,
            JobVersion        = jobVersion,
            UserName          = startSpec.CallerUser,
            UserDomain        = startSpec.CallerDomain,
            ClientApplication = startSpec.CallerApplication,
            ClientMachine     = startSpec.CallerMachine,
            TecUser           = startSpec.TechnicalUserName,
            Started           = DateTime.Now,
            ExecutionStatus   = ExecutionStatus.Preparing,
            JapServerProcess  = Environment.ProcessId,
            JobHostProcessId  = 0,
            JapExitCode       = 0,
            JobExitCode       = 0,
        });

        if (!save.Succeeded)
        {
            ctx.SessionGuid = Guid.Empty; // record was never persisted — no valid session to reference
            ctx.InnerError  = save;
            return;
        }

        var getSource = ResolveConfigSource(bootStrapConfig);
        if (!getSource.Succeeded)
        {
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = getSource.ToResult();
            return;
        }
        var configStore = new ConfigStore(getSource.Value, bootStrapConfig.SessionStoreConnectionString);

        ctx.JapServer = new JAPServer(sessionStore, ctx.SessionGuid, jobName, errorHandler, configStore);
        var dispatcherResult = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(ctx.JapServer);
        if (!dispatcherResult.Succeeded)
        {
            SetTerminalStatus(sessionStore, ctx.SessionGuid, ExecutionStatus.FinishedFaulted, errorHandler, jobName);
            ctx.InnerError = dispatcherResult.ToResult();
            return;
        }
        var jipDispatcher = dispatcherResult.Value;

        var serverStartArguments = new ServerStartArguments
        {
            ClientExePath           = jobExePath,
            ConnectionTimeout       = TimeSpan.FromMinutes(1),
            HandleStringRequest     = requestStr => HandleRequest(jipDispatcher, requestStr),
            SessionKey              = ctx.SessionGuid,
            HandleErrorNotification = (req, ex) => HandleHandlerError(req, ex, jobName, ctx.SessionGuid, errorHandler),
            IsCancellationRequested = () => Task.FromResult(ctx.ShouldCancelServer)
        };

        ctx.ServerTask = PipesServer.RunAsync(serverStartArguments);

        //
        // Await negotiation outcome with 30-second timeout 
        //
        var winner = await Task.WhenAny(ctx.JapServer.NegotiationOutcome, Task.Delay(TimeSpan.FromSeconds(30)));

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
    

    private static FileBootstrapConfig? LoadBootstrapConfig()
    {
        //
        // Bootstrapping -> noch keine ErrorHandler, also Fehler direkt loggen und mit null zurückgeben. 
        //
        var loadConfig = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName));
        if (loadConfig.Succeeded) return loadConfig.Value;
        var err = $"Bootstrap-Konfiguration konnte nicht geladen werden: {loadConfig.ErrorMessage}";
        LocalLog.WriteError(err);   
        return null;
    }

    private static Result<SessionStartSpec> GetSessionStartSpec(string filePathArgument)
    {
        var readRawStartSpec = ReadRawSessionStartSpec(filePathArgument);
        if (!readRawStartSpec.Succeeded)
            return Result<SessionStartSpec>.Propagate(readRawStartSpec.ToResult<SessionStartSpec>());

        var getSpec = DeserializeSessionStartSpec(readRawStartSpec.Value);
        return getSpec;
    }

    private static Result<string> ReadRawSessionStartSpec(string filePathArgument)
    {
        try
        {
            if (!filePathArgument.StartsWith('@'))
                return Result<string>.Fail("JOSYN-START: Dateiargument muss mit '@' beginnen.");
            
            var filePath = filePathArgument[1..];
            var raw = File.ReadAllText(filePath);
            try
            {
                File.Delete(filePath);
            }
            catch { /* ignore */ }

            return raw;
        }
        catch (Exception ex) 
        { 
            return Result<string>.Fail($"JOSYN-START: SessionStartSpec-Datei konnte nicht gelesen werden: '{filePathArgument}'", ex);
        }
    }
   
    private static Result<SessionStartSpec> DeserializeSessionStartSpec(string raw)
    {
        var deserialize = PropertyBag.Deserialize<SessionStartSpec>(raw);
        if (deserialize.Succeeded) return deserialize.Value;
        return Result<SessionStartSpec>.Propagate(deserialize.ToResult<SessionStartSpec>());
    }
    
    private static Result<string> Base64DecodeArgumentsString(string base64String)
    {
        try
        {
            var result = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
            return result;
        }
        catch (Exception ex) { return ex; }
    }

    
}
