using JOSYN.Backend.SessionLauncherContract;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionLauncher;

/// <summary>
/// Orchestrator-side launcher for JOSYN job sessions.
/// Validates job registration, resolves <c>TechnicalUserName</c>, serializes
/// <see cref="SessionStartRequest"/>, writes a temp file, and spawns
/// <c>JAPServer.exe JOSYN-START @&lt;path&gt;</c>.
/// </summary>
public interface ISessionLauncher
{
    /// <summary>
    /// Validates the job, populates <c>TechnicalUserName</c>, and spawns JAPServer.
    /// </summary>
    /// <param name="request">
    /// The session-start request. <c>TechnicalUserName</c> is ignored — it will be
    /// resolved from <c>JobRegistry</c> and overwritten by the implementation.
    /// </param>
    Result LaunchSession(SessionStartRequest request);
}
