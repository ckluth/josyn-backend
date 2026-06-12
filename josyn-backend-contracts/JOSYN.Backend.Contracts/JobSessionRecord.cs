namespace JOSYN.Backend.Contracts;

/// <summary>
/// Concrete implementation of <see cref="IJobSessionRecord"/> (see ADR-007).
/// </summary>
public sealed record JobSessionRecord : IJobSessionRecord
{
    public required Guid            UID               { get; init; }
    public required string          JobTypeName       { get; init; }
    public required string          Arguments         { get; init; }
    public required string          Result            { get; init; }
    public required string          JobVersion        { get; init; }
    public required string          UserName          { get; init; }
    public required string          UserDomain        { get; init; }
    public required string          ClientApplication { get; init; }
    public required string          ClientMachine     { get; init; }
    public          string?         TecUser           { get; init; }
    public required DateTime        Started           { get; init; }
    public required ExecutionStatus ExecutionStatus   { get; init; }
    public          string?         Progress          { get; init; }
    public          DateTime?       Finished          { get; init; }
    public required int             JapServerProcess  { get; init; }
    public required int             JobHostProcessId  { get; init; }
    public required int             JapExitCode       { get; init; }
    public required int             JobExitCode       { get; init; }
    public          DateTime?       LastWriteTime     { get; init; }
    public          string?         WrittenBy         { get; init; }
}
