using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

public interface IJobRegistry
{
    Result<IJobRegistrationRecord> GetByName(string name);
    Result<IReadOnlyList<IJobRegistrationRecord>> GetAll();
}
