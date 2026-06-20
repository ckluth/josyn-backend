namespace JOSYN.Backend.JobRegistry;

public sealed record JobRegistrationRecord : IJobRegistrationRecord
{
    public required string                         Name              { get; init; }
    public required string                         TechnicalUserName { get; init; }
    public required IReadOnlyList<IArgumentRecord> ArgumentRecords   { get; init; }
}
