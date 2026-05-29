using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStarter;

/// <summary>
/// Creates a new job session: persists the session record, spawns
/// <c>JAPServer.exe JOSYN-IPC &lt;sessionGUID&gt;</c>, and spawns
/// <c>job.exe JOSYN-IPC &lt;sessionGUID&gt;</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is the migration rendezvous between the legacy backend and the JOSYN platform.
/// In the old system, <c>SessionStarter</c> called
/// <c>Process.Start(JobHost.exe, sessionUID)</c>.
/// In the new system it spawns <c>JAPServer.exe</c> and the job executable with the
/// same session GUID, allowing them to rendezvous via named pipes.
/// </para>
/// <para>
/// <c>josyn-backend</c> owns the session lifecycle:
/// scheduling, session-store persistence, and process spawning.
/// <c>josyn-system</c> owns only the per-session JAP protocol server.
/// </para>
/// </remarks>
public interface ISessionStarter
{
    /// <summary>
    /// Creates a new session entry in the session store and launches both
    /// <c>JAPServer.exe</c> and <c>job.exe</c> with the freshly assigned session GUID.
    /// </summary>
    /// <param name="request">The session start parameters.</param>
    /// <returns>The fresh session GUID on success; an error on failure.</returns>
    static abstract Result<Guid> StartSession(StartSessionRequest request);
}
