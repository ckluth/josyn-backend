using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Backend.JobScheduleStore;
#pragma warning restore IDE0130

public interface IFiredSlotStore
{
    /// <summary>
    /// Inserts a new fired-slot record if no record with the same deduplication key
    /// <c>(JobName, ArgumentRecordName, SlotTime)</c> exists.
    /// Returns <c>true</c> when the row was inserted (first fire for this slot);
    /// <c>false</c> when a duplicate key was found (slot already handled by a previous tick).
    /// </summary>
    Result<bool> TryInsert(string jobName, string argumentRecordName, DateTime slotTime, DateTime firedAt);

    /// <summary>
    /// Deletes all rows whose <c>SlotTime</c> is strictly older than <paramref name="cutoff"/>.
    /// </summary>
    Result Prune(DateTime cutoff);
}
