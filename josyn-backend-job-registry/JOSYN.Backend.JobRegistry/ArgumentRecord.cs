namespace JOSYN.Backend.JobRegistry;

public sealed record ArgumentRecord : IArgumentRecord
{
    public required string JobName  { get; init; }
    public required string Name     { get; init; }
    public required string Content  { get; init; }
}
