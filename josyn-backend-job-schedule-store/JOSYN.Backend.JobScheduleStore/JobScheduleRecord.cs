namespace JOSYN.Backend.JobScheduleStore;

public sealed record JobScheduleRecord : IJobScheduleRecord
{
    public required string                                 JobName        { get; init; }
    public required bool                                   Suspended      { get; init; }
    public required DateOnly?                              SuspendedUntil { get; init; }
    public required IReadOnlyList<IJobScheduleEntryRecord> Entries        { get; init; }
}
