using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStore;

public interface ISessionStore
{
    Result SaveNewSession(IJobSessionRecord jobSession);

    Result<IJobSessionRecord> GetSession(Guid sessionUid);

    Result UpdateSession(IJobSessionRecord jobSession);
}
