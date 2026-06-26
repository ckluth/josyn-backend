using JOSYN.Backend.Contracts;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStore;

public interface ISessionStore
{
    Result SaveNewSession(IJobSessionRecord jobSession);

    Result<IJobSessionRecord> GetSession(Guid sessionUid);

    Result UpdateSession(IJobSessionRecord jobSession);

    /// <summary>
    /// Returns the most recent sessions, ordered by <see cref="IJobSessionRecord.Started"/> descending.
    /// </summary>
    /// <param name="maxCount">Maximum number of sessions to return. Must be greater than zero.</param>
    Result<IReadOnlyList<IJobSessionRecord>> GetRecentSessions(int maxCount);

    /// <summary>
    /// Returns the raw arguments of all sessions of <paramref name="jobTypeName"/>
    /// that are currently in a transient state (<c>preparing</c>, <c>running</c>,
    /// or <c>running-cancellation-requested</c>), excluding the session identified
    /// by <paramref name="excludeSessionGuid"/>.
    /// Used to evaluate parallel-execution policy during session start negotiation (ADR-008).
    /// </summary>
    Result<IReadOnlyList<string>> GetConcurrentSessionArguments(Guid excludeSessionGuid, string jobTypeName);
}
