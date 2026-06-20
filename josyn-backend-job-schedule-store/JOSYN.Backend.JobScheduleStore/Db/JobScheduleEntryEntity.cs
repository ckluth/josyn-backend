namespace JOSYN.Backend.JobScheduleStore;

internal sealed class JobScheduleEntryEntity
{
    public string JobName            { get; set; } = string.Empty;
    public string ArgumentRecordName { get; set; } = string.Empty;
    public string ScheduleDefinition { get; set; } = string.Empty;

    public JobScheduleEntity? Schedule { get; set; }
}
