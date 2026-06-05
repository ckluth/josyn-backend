using System.Diagnostics;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStarter;

/// <inheritdoc/>
public sealed class SessionStarter(
    ISessionStore sessionStore,
    IBootstrapConfig bootstrapConfig,
    IJobRegistry  jobRegistry) : ISessionStarter
{
    /// <inheritdoc/>
    public Result<Guid> StartSession(string jobTypeName, string arguments)
    {
        var jobCheck = jobRegistry.GetByName(jobTypeName);
        if (!jobCheck.Succeeded)
            return Result.Error($"Job nicht registriert: '{jobTypeName}'. Bitte zuerst in josyn.JobRegistry eintragen.");

        var exePath = bootstrapConfig.JapServerExePath;
        if (!File.Exists(exePath))
            return Result.Error($"JAPServer-Executable nicht gefunden: '{exePath}'");

        var sessionGuid = Guid.NewGuid();

        var save = sessionStore.SaveNewSession(new JobSessionRecord
        {
            UID         = sessionGuid,
            JobTypeName = jobTypeName,
            Arguments   = arguments,
            Result      = string.Empty
        });

        if (!save.Succeeded)
            return Result<Guid>.Propagate(save.ToResult<Guid>());

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName        = exePath,
                Arguments       = $"JOSYN-IPC {sessionGuid}",
                UseShellExecute = false
            });

            return sessionGuid;
        }
        catch (Exception ex) { return ex; }
    }
}
