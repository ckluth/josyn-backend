using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Backend.JobScheduleStore;
#pragma warning restore IDE0130

public interface IJobScheduleStore
{
    /// <summary>
    /// Returns all job schedules together with their entries.
    /// Called once per TimeScheduler invocation; evaluation happens locally.
    /// </summary>
    Result<IReadOnlyList<IJobScheduleRecord>> GetAll();
}
