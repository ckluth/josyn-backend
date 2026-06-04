using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.SessionStore;

public sealed class SessionStore(string connectionString) : ISessionStore
{
    public Result SaveNewSession(IJobSessionRecord jobSession)
    {
        try
        {
            using var ctx = new SessionStoreDbContext(connectionString);
            ctx.SessionStore.Add(new SessionStoreEntity
            {
                UID         = jobSession.UID,
                JobTypeName = jobSession.JobTypeName,
                Arguments   = jobSession.Arguments,
                Result      = jobSession.Result
            });
            ctx.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }

    public Result<IJobSessionRecord> GetSession(Guid sessionUid)
    {
        try
        {
            using var ctx = new SessionStoreDbContext(connectionString);
            var entity = ctx.SessionStore
                .AsNoTracking()
                .FirstOrDefault(e => e.UID == sessionUid);

            if (entity is null)
                return Result.Error($"No session found for UID '{sessionUid}'.");

            return new JobSessionRecord
            {
                UID         = entity.UID,
                JobTypeName = entity.JobTypeName,
                Arguments   = entity.Arguments,
                Result      = entity.Result
            };
        }
        catch (Exception ex) { return ex; }
    }

    public Result UpdateSession(IJobSessionRecord jobSession)
    {
        try
        {
            using var ctx = new SessionStoreDbContext(connectionString);
            var entity = ctx.SessionStore.FirstOrDefault(e => e.UID == jobSession.UID);

            if (entity is null)
                return Result.Error($"No session found for UID '{jobSession.UID}'.");

            entity.JobTypeName = jobSession.JobTypeName;
            entity.Arguments   = jobSession.Arguments;
            entity.Result      = jobSession.Result;
            ctx.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }
}
