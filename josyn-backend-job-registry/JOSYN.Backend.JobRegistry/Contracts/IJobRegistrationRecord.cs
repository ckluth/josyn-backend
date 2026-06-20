#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

public interface IJobRegistrationRecord
{
    string                           Name              { get; }
    string                           TechnicalUserName { get; }
    IReadOnlyList<IArgumentRecord>   ArgumentRecords   { get; }
}
