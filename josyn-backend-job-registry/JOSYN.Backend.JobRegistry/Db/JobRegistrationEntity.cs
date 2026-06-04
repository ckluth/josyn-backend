namespace JOSYN.Backend.JobRegistry;

internal sealed class JobRegistrationEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TechnicalUserName { get; set; } = string.Empty;
}
