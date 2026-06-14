using System.Diagnostics;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.Contracts;
using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionLauncher;

/// <inheritdoc/>
public class SessionLauncher : ISessionLauncher
{
    private const string tempFileNamePattern = "josyn-start-{0}.ini";
    
    public static Result LaunchSession(SessionLaunchRequest request, string backendRoot, IJobRegistry jobRegistry)
    {
        var jobCheck = jobRegistry.GetByName(request.JobTypeName);
        if (!jobCheck.Succeeded)
            return Result.Error($"Job nicht registriert: '{request.JobTypeName}'. Bitte zuerst in josyn.JobRegistry eintragen.");

        var spec = new SessionStartSpec
        {
            JobTypeName = request.JobTypeName,
            Arguments = request.Arguments,
            TechnicalUserName = jobCheck.Value.TechnicalUserName,
            CallerUser = request.CallerUser,
            CallerDomain = request.CallerDomain,
            CallerApplication = request.CallerApplication,
            CallerMachine = request.CallerMachine
        };
        
        var japServerExePath = Path.Combine(backendRoot, JapServerConstants.FolderName, JapServerConstants.ExeName);
        if (!File.Exists(japServerExePath))
            return Result.Error($"JAPServer-Executable nicht gefunden: '{japServerExePath}'");

        var tempJobArgumentsFilePath = Path.Combine(Path.GetTempPath(), string.Format(tempFileNamePattern, Guid.NewGuid()));

        var serialize = PropertyBag.Serialize(spec);
        if (!serialize.Succeeded)
            return Result.Error(serialize.ErrorMessage!);

        try
        {
            File.WriteAllText(tempJobArgumentsFilePath, serialize.Value);

            Process.Start(new ProcessStartInfo
            {
                FileName = japServerExePath,
                Arguments = $"{JapServerConstants.CliModeStart} \"@{tempJobArgumentsFilePath}\"",
                UseShellExecute = false
            });

            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }
}
