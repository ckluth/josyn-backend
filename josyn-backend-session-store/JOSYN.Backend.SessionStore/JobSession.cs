namespace JOSYN.Backend.SessionStore;

public sealed record JobSessionRecord : IJobSessionRecord
{
    public required Guid UID { get; init; }
    public required string JobTypeName { get; init; }
    public required string Arguments { get; init; }
    public required string Result { get; init; }
}
