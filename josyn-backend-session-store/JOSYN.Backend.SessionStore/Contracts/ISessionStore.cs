using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SessionStore;

public interface ISessionStore
{
    Result SaveNewSession(IJobSession jobSession);

    Result<IJobSession> GetSession(Guid sessionUid);

    Result UpdateSession(IJobSession jobSession);
}
