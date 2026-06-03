using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStarter;

/// <summary>
/// Starts a new job session: persists it in the session store and spawns
/// <c>JAPServer.exe</c> to handle the session lifecycle.
/// </summary>
public interface ISessionStarter
{
    /// <summary>
    /// Allocates a session GUID, saves the session to the store, and spawns
    /// <c>JAPServer.exe JOSYN-IPC &lt;guid&gt;</c>.
    /// </summary>
    /// <param name="jobTypeName">Identifies the job type to execute.</param>
    /// <param name="arguments">Serialized job arguments (INI format).</param>
    /// <returns>The session GUID on success; an error result otherwise.</returns>
    Result<Guid> StartSession(string jobTypeName, string arguments);
}
