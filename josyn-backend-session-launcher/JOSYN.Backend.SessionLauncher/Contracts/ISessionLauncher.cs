using JOSYN.Backend.Contracts;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionLauncher;

/// <summary>
/// Orchestrator-side launcher for JOSYN job sessions.
/// Validates job registration, resolves <c>TechnicalUserName</c>, builds a
/// <see cref="SessionStartSpec"/>, writes it to a temp file, and spawns
/// <c>JAPServer.exe JOSYN-START @&lt;path&gt;</c>.
/// </summary>
public interface ISessionLauncher
{
    /// <summary>
    /// Validates the job, resolves <c>TechnicalUserName</c> from <c>JobRegistry</c>,
    /// and spawns JAPServer.
    /// </summary>
    /// <param name="request">The session launch request from the orchestrator.</param>
    Result LaunchSession(SessionLaunchRequest request);
}
