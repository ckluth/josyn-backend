#pragma warning disable IDE0130
namespace JOSYN.Backend.JobScheduleStore;
#pragma warning restore IDE0130

public interface IJobScheduleEntryRecord
{
    string ArgumentRecordName { get; }
    string ScheduleDefinition { get; }
}
