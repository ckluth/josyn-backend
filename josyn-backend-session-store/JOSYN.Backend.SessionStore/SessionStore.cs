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
                UID               = jobSession.UID,
                JobTypeName       = jobSession.JobTypeName,
                Arguments         = jobSession.Arguments,
                Result            = jobSession.Result,
                JobVersion        = jobSession.JobVersion,
                UserName          = jobSession.UserName,
                UserDomain        = jobSession.UserDomain,
                ClientApplication = jobSession.ClientApplication,
                ClientMachine     = jobSession.ClientMachine,
                TecUser           = jobSession.TecUser,
                Started           = jobSession.Started,
                ExecutionStatus   = jobSession.ExecutionStatus,
                Progress          = jobSession.Progress,
                Finished          = jobSession.Finished,
                JapServerProcess  = jobSession.JapServerProcess,
                JobHostProcessId  = jobSession.JobHostProcessId,
                JapExitCode       = jobSession.JapExitCode,
                JobExitCode       = jobSession.JobExitCode,
                LastWriteTime     = DateTime.Now,
                WrittenBy         = jobSession.WrittenBy
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
                UID               = entity.UID,
                JobTypeName       = entity.JobTypeName,
                Arguments         = entity.Arguments,
                Result            = entity.Result,
                JobVersion        = entity.JobVersion,
                UserName          = entity.UserName,
                UserDomain        = entity.UserDomain,
                ClientApplication = entity.ClientApplication,
                ClientMachine     = entity.ClientMachine,
                TecUser           = entity.TecUser,
                Started           = entity.Started,
                ExecutionStatus   = entity.ExecutionStatus,
                Progress          = entity.Progress,
                Finished          = entity.Finished,
                JapServerProcess  = entity.JapServerProcess,
                JobHostProcessId  = entity.JobHostProcessId,
                JapExitCode       = entity.JapExitCode,
                JobExitCode       = entity.JobExitCode,
                LastWriteTime     = entity.LastWriteTime,
                WrittenBy         = entity.WrittenBy
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

            entity.JobTypeName       = jobSession.JobTypeName;
            entity.Arguments         = jobSession.Arguments;
            entity.Result            = jobSession.Result;
            entity.JobVersion        = jobSession.JobVersion;
            entity.UserName          = jobSession.UserName;
            entity.UserDomain        = jobSession.UserDomain;
            entity.ClientApplication = jobSession.ClientApplication;
            entity.ClientMachine     = jobSession.ClientMachine;
            entity.TecUser           = jobSession.TecUser;
            entity.Started           = jobSession.Started;
            entity.ExecutionStatus   = jobSession.ExecutionStatus;
            entity.Progress          = jobSession.Progress;
            entity.Finished          = jobSession.Finished;
            entity.JapServerProcess  = jobSession.JapServerProcess;
            entity.JobHostProcessId  = jobSession.JobHostProcessId;
            entity.JapExitCode       = jobSession.JapExitCode;
            entity.JobExitCode       = jobSession.JobExitCode;
            entity.LastWriteTime     = DateTime.Now;
            entity.WrittenBy         = jobSession.WrittenBy;
            ctx.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }
}
