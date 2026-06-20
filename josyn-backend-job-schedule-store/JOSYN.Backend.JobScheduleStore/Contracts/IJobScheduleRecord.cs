#pragma warning disable IDE0130
namespace JOSYN.Backend.JobScheduleStore;
#pragma warning restore IDE0130

public interface IJobScheduleRecord
{
    string                                 JobName        { get; }
    bool                                   Suspended      { get; }
    DateOnly?                              SuspendedUntil { get; }
    IReadOnlyList<IJobScheduleEntryRecord> Entries        { get; }
}
