using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.JobScheduleStore;

public sealed class SqlFiredSlotStore(string connectionString) : IFiredSlotStore
{
    public Result<bool> TryInsert(string jobName, string argumentRecordName, DateTime slotTime, DateTime firedAt)
    {
        try
        {
            using var ctx = new FiredSlotStoreDbContext(connectionString);
            // At-most-once (ADR-029 §6): insert only when the deduplication key is absent.
            // Multiple ticks within [S, S+T] all resolve to the same S and hit the same PK.
            var rows = ctx.Database.ExecuteSql(
                $@"INSERT INTO [josyn].[FiredSlots] ([JobName], [ArgumentRecordName], [SlotTime], [FiredAt])
                   SELECT {jobName}, {argumentRecordName}, {slotTime}, {firedAt}
                   WHERE NOT EXISTS (
                       SELECT 1 FROM [josyn].[FiredSlots]
                       WHERE [JobName]            = {jobName}
                         AND [ArgumentRecordName] = {argumentRecordName}
                         AND [SlotTime]           = {slotTime}
                   )");

            return Result<bool>.Success(rows == 1);
        }
        catch (Exception ex) { return ex; }
    }

    public Result Prune(DateTime cutoff)
    {
        try
        {
            using var ctx = new FiredSlotStoreDbContext(connectionString);
            ctx.Database.ExecuteSql($"DELETE FROM [josyn].[FiredSlots] WHERE [SlotTime] < {cutoff}");
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }
}
