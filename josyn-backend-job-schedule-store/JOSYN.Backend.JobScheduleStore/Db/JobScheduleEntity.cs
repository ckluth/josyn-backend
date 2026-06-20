namespace JOSYN.Backend.JobScheduleStore;

internal sealed class JobScheduleEntity
{
    public string   JobName        { get; set; } = string.Empty;
    public bool     Suspended      { get; set; }
    public DateOnly? SuspendedUntil { get; set; }

    public ICollection<JobScheduleEntryEntity> Entries { get; set; } = [];
}
