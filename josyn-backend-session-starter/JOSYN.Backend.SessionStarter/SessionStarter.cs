using System.Diagnostics;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.Contracts;
using JOSYN.Backend.SessionStore;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStarter;

/// <inheritdoc/>
public sealed class SessionStarter(
    ISessionStore    sessionStore,
    IBootstrapConfig bootstrapConfig,
    IJobRegistry     jobRegistry) : ISessionStarter
{
    private readonly IBootstrapConfig _bootstrapConfig = bootstrapConfig;
    /// <summary>
    /// Resolves the JAPServer executable path by convention:
    /// <c>BackendRoot\JAPServer\JOSYN.Jap.JAPServer.exe</c>,
    /// where BackendRoot is the directory containing <c>josyn.bootstrap.ini</c> (ADR-012).
    /// </summary>
    private string JapServerExePath =>
        Path.Combine(_bootstrapConfig.BackendRoot, JapServerConstants.FolderName, JapServerConstants.ExeName);

    /// <inheritdoc/>
    public Result<Guid> StartSession(string jobTypeName, string arguments)
    {
        var jobCheck = jobRegistry.GetByName(jobTypeName);
        if (!jobCheck.Succeeded)
            return Result.Error($"Job nicht registriert: '{jobTypeName}'. Bitte zuerst in josyn.JobRegistry eintragen.");

        var exePath = JapServerExePath;
        if (!File.Exists(exePath))
            return Result.Error($"JAPServer-Executable nicht gefunden: '{exePath}'");

        var sessionGuid = Guid.NewGuid();

        var save = sessionStore.SaveNewSession(new JobSessionRecord
        {
            UID               = sessionGuid,
            JobTypeName       = jobTypeName,
            Arguments         = arguments,
            Result            = string.Empty,
            JobVersion        = string.Empty,
            UserName          = Environment.UserName,
            UserDomain        = Environment.UserDomainName,
            ClientApplication = AppDomain.CurrentDomain.FriendlyName,
            ClientMachine     = Environment.MachineName,
            TecUser           = jobCheck.Value.TechnicalUserName,
            Started           = DateTime.UtcNow,
            ExecutionStatus   = ExecutionStatus.Pending,
            JapServerProcess  = 0,
            JobHostProcessId  = 0,
            JapExitCode       = 0,
            JobExitCode       = 0
        });

        if (!save.Succeeded)
            return Result<Guid>.Propagate(save.ToResult<Guid>());

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName        = exePath,
                Arguments       = PipesProtocol.CreateClientStartCLIArguments(sessionGuid.ToString()),
                UseShellExecute = false
            });

            return sessionGuid;
        }
        catch (Exception ex) { return ex; }
    }
}
