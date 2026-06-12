using System.Diagnostics;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.Contracts;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionLauncher;

/// <inheritdoc/>
public sealed class SessionLauncher(
    IBootstrapConfig bootstrapConfig,
    IJobRegistry     jobRegistry) : ISessionLauncher
{
    private readonly IBootstrapConfig _bootstrapConfig = bootstrapConfig;

    private string JapServerExePath =>
        Path.Combine(_bootstrapConfig.BackendRoot, "JAPServer", "JOSYN.Jap.JAPServer.exe");

    /// <inheritdoc/>
    public Result LaunchSession(SessionStartRequest request)
    {
        var jobCheck = jobRegistry.GetByName(request.JobTypeName);
        if (!jobCheck.Succeeded)
            return Result.Error($"Job nicht registriert: '{request.JobTypeName}'. Bitte zuerst in josyn.JobRegistry eintragen.");

        var resolvedRequest = request with { TechnicalUserName = jobCheck.Value.TechnicalUserName };

        var exePath = JapServerExePath;
        if (!File.Exists(exePath))
            return Result.Error($"JAPServer-Executable nicht gefunden: '{exePath}'");

        var tempFile = Path.Combine(Path.GetTempPath(), $"josyn-start-{Guid.NewGuid()}.ini");

        var serialize = PropertyBag.Serialize(resolvedRequest);
        if (!serialize.Succeeded)
            return Result.Error(serialize.ErrorMessage!);

        try
        {
            File.WriteAllText(tempFile, serialize.Value);

            Process.Start(new ProcessStartInfo
            {
                FileName        = exePath,
                Arguments       = $"JOSYN-START @{tempFile}",
                UseShellExecute = false
            });

            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }
}
