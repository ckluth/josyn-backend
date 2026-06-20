namespace JOSYN.Backend.JobScheduleStore;

public sealed record JobScheduleEntryRecord : IJobScheduleEntryRecord
{
    public required string ArgumentRecordName { get; init; }
    public required string ScheduleDefinition { get; init; }
    public          int?   ToleranceMinutes   { get; init; }
}
