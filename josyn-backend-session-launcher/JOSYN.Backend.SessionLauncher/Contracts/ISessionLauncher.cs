using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.Contracts;
using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Backend.SessionLauncher;
#pragma warning restore IDE0130

/// <summary>
/// Orchestrator-side launcher for JOSYN job sessions.
/// Validates job registration, resolves <c>TechnicalUserName</c>, builds a
/// <see cref="SessionStartSpec"/>, writes it to a temp file, and spawns
/// <c>SessionBroker.exe JOSYN-START @&lt;path&gt;</c>.
/// </summary>
public interface ISessionLauncher
{
    /// <summary>
    /// Validates the job, resolves <c>TechnicalUserName</c> from <c>JobRegistry</c>,
    /// and spawns SessionBroker.
    /// </summary>
    /// <param name="request">The session launch request from the orchestrator.</param>
    /// <param name="backendRoot">The backend root path for locating SessionBroker.exe.</param> 
    /// <param name="jobRegistry">The job registry.</param>
    public static abstract Result LaunchSession(SessionLaunchRequest request, string backendRoot, IJobRegistry jobRegistry);
}
