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

    //
    // The JAP-Server-Entrypoint for running a new Job-Session...
    //
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

        var spawnAdapters = await SpawnAdapters(bootStrapConfig);
        if (!spawnAdapters.Succeeded)
        {
            errorHandler.Handle(spawnAdapters.ToResult());
            return 1;
        }

        using var adapterManager = spawnAdapters.Value;
        var sessionStartSpecFilepath = args[1];
        return await ProcessSessionStart(sessionStartSpecFilepath, bootStrapConfig, errorHandler, adapterManager);

        //
        // nested function
        //
        static FileBootstrapConfig? LoadBootstrapConfig()
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
    }

    //
    // Verarbeitet den JOSYN-START-Modus: Liest die SessionStartSpec, führt die Turnstile-Logik
    // für die Session-Akzeptanz durch und wartet nach dem Turnstile auf den JAP-Serve-Loop.
    //
    private static async Task<int> ProcessSessionStart(string sessionStartSpecFilepath, IBootstrapConfig bootStrapConfig, IErrorHandler errorHandler, AdapterManager adapterManager)
    {
        var getSpec = GetSessionStartSpec(sessionStartSpecFilepath);
        if (!getSpec.Succeeded) { errorHandler.Handle(getSpec.ToResult()); return 1; }
        var startSpec = getSpec.Value;

        var getPlainArguments = Base64DecodeArgumentsString(startSpec.Arguments);
        if (!getPlainArguments.Succeeded) { errorHandler.Handle(getPlainArguments.ToResult()); return 1; }

        var sessionStore = new SessionStore(bootStrapConfig.SessionStoreConnectionString);
        var jobName = startSpec.JobTypeName;
        var jobExePath = Path.Combine(bootStrapConfig.BackendRoot, JobRepositoryFolder, jobName, jobName + ".exe");

        //
        // Turnstile scope: GUID allocation → session persistence → spawn → accept/reject negotiation.
        // Released only when the session is definitively in-flight (running) or closed (finished-rejected).
        //
        var turnstileResult = await Turnstile.RunAsync<PrepareContext>(jobName, () => Prepare(sessionStore, jobName, jobExePath, bootStrapConfig, startSpec, getPlainArguments.Value, errorHandler, adapterManager));
        if (!turnstileResult.Succeeded)
        {
            errorHandler.Handle(turnstileResult.ToResult(), sessionGuid: null);
            return 1;
        }

        var ctx = turnstileResult.Value;
        if (!ctx.InnerError.Succeeded)
        {
            errorHandler.Handle(ctx.InnerError, sessionGuid: ctx.SessionGuid); 
            return 1;
        }

        // Parallel Execution zurückgewiesen?
        if (!ctx.NegotiationAccepted) return 0;

        //
        // Session accepted and running — await JAP serve loop.
        // ServerTask is non-null: assigned in PrepareServer and only nulled in the
        // timeout/rejected early-return branches, both of which exit before
        // NegotiationAccepted can be set to true. The guard above ensures we only
        // reach this line via the accepted path.
        //
        var res = await ctx.ServerTask!;

        //
        // Finalization — write terminal status only if the protocol did not already do so.
        // The job may have reported its own final status via a JAP protocol message during
        // the server task; writing again here would silently overwrite that value.
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
        
        
        //================================================================

        //
        // nested helper function...
        //
        static Result<SessionStartSpec> GetSessionStartSpec(string filePathArgument)
        {
            var readRawStartSpec = ReadRawSessionStartSpec(filePathArgument);
            if (!readRawStartSpec.Succeeded)
                return Result<SessionStartSpec>.Propagate(readRawStartSpec.ToResult<SessionStartSpec>());

            var getSpec = DeserializeSessionStartSpec(readRawStartSpec.Value);
            return getSpec;

            //
            // nested helper function...
            //            
            static Result<string> ReadRawSessionStartSpec(string filePathArgument)
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
            
            //
            // nested helper function...
            //            
            static Result<SessionStartSpec> DeserializeSessionStartSpec(string raw)
            {
                var deserialize = PropertyBag.Deserialize<SessionStartSpec>(raw);
                if (deserialize.Succeeded) return deserialize.Value;
                return Result<SessionStartSpec>.Propagate(deserialize.ToResult<SessionStartSpec>());
            }
        }

        //
        // nested helper function...
        //        
        static Result<string> Base64DecodeArgumentsString(string base64String)
        {
            try
            {
                var result = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
                return result;
            }
            catch (Exception ex) { return ex; }
        }
    }
}
