namespace JOSYN.Backend.JobScheduleStore;

internal sealed class FiredSlotEntity
{
    public string   JobName            { get; set; } = string.Empty;
    public string   ArgumentRecordName { get; set; } = string.Empty;
    public DateTime SlotTime           { get; set; }
    public DateTime FiredAt            { get; set; }
}
