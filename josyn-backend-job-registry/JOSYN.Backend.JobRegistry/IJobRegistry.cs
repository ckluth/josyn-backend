using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.JobRegistry;

public interface IJobRegistry
{
    Result<IJobRegistrationRecord>               GetByName(string name);
    Result<IReadOnlyList<IJobRegistrationRecord>> GetAll();
}
