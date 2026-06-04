namespace JOSYN.Backend.JobRegistry;

public interface IJobRegistrationRecord
{
    string Name { get; }
    string TechnicalUserName { get; }
}
