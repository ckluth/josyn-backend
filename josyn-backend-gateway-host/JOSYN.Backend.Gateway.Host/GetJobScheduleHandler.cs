using JOSYN.Backend.JobScheduleStore;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <see cref="GetJobSchedule"/>: the schedule and all its entries for one registered job.
/// </summary>
/// <remarks>
/// The store returns all schedules (<see cref="IJobScheduleStore.GetAll"/>); the target job is
/// identified by host-side filter. This avoids adding a single-job lookup to the store API and
/// is acceptable because the schedule table is small (one row per job).
/// </remarks>
internal sealed class GetJobScheduleHandler(string connectionString)
{
    internal Result<JobSchedule> Handle(GetJobSchedule query)
    {
        IJobScheduleStore store = new SqlJobScheduleStore(connectionString);
        var storeResult = store.GetAll();
        if (!storeResult.Succeeded)
            return storeResult.ToResult<JobSchedule>();

        var record = storeResult.Value.FirstOrDefault(r => r.JobName == query.JobName);
        if (record is null)
            return JrpError.NotFound(
                $"No schedule found for job '{query.JobName}' on this installation.");

        return Result<JobSchedule>.Success(MapSchedule(record, query));

        // ── helpers ──────────────────────────────────────────────────────────
        static JobSchedule MapSchedule(IJobScheduleRecord r, GetJobSchedule q) => new()
        {
            Environment    = q.Target.Environment,
            Machine        = q.Target.Machine,
            JobName        = r.JobName,
            Suspended      = r.Suspended,
            SuspendedUntil = r.SuspendedUntil,
            // Entries are sorted by argument-record name so ordering is stable.
            Entries = r.Entries
                .OrderBy(e => e.ArgumentRecordName)
                .Select(e => new ScheduleEntrySummary
                {
                    ArgumentRecordName = e.ArgumentRecordName,
                    ScheduleDefinition = e.ScheduleDefinition,
                    ToleranceMinutes   = e.ToleranceMinutes
                })
                .ToList()
        };
    }
}
