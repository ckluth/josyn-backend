using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.JobScheduleStore;

public sealed class SqlJobScheduleStore(string connectionString) : IJobScheduleStore
{
    public Result<IReadOnlyList<IJobScheduleRecord>> GetAll()
    {
        try
        {
            using var ctx = new JobScheduleStoreDbContext(connectionString);
            var entities = ctx.JobSchedules
                .AsNoTracking()
                .Include(e => e.Entries)
                .OrderBy(e => e.JobName)
                .ToList();

            IReadOnlyList<IJobScheduleRecord> records = entities
                .Select(e => (IJobScheduleRecord)ToRecord(e))
                .ToList();

            return Result<IReadOnlyList<IJobScheduleRecord>>.Success(records);
        }
        catch (Exception ex) { return ex; }
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    private static JobScheduleRecord ToRecord(JobScheduleEntity entity) =>
        new()
        {
            JobName        = entity.JobName,
            Suspended      = entity.Suspended,
            SuspendedUntil = entity.SuspendedUntil,
            Entries        = entity.Entries
                .Select(e => (IJobScheduleEntryRecord)new JobScheduleEntryRecord
                {
                    ArgumentRecordName = e.ArgumentRecordName,
                    ScheduleDefinition = e.ScheduleDefinition
                })
                .ToList()
        };
}
