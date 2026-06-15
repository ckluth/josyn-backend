namespace JOSYN.Backend.Contracts;

/// <summary>
/// Concrete implementation of <see cref="IJobSessionRecord"/> (see ADR-007).
/// </summary>
public sealed record JobSessionRecord : IJobSessionRecord
{
    /// <inheritdoc/>
    public required Guid            UID               { get; init; }
    /// <inheritdoc/>
    public required string          JobTypeName       { get; init; }
    /// <inheritdoc/>
    public required string          Arguments         { get; init; }
    /// <inheritdoc/>
    public required string          Result            { get; init; }
    /// <inheritdoc/>
    public required string          JobVersion        { get; init; }
    /// <inheritdoc/>
    public required string          UserName          { get; init; }
    /// <inheritdoc/>
    public required string          UserDomain        { get; init; }
    /// <inheritdoc/>
    public required string          ClientApplication { get; init; }
    /// <inheritdoc/>
    public required string          ClientMachine     { get; init; }
    /// <inheritdoc/>
    public          string?         TecUser           { get; init; }
    /// <inheritdoc/>
    public required DateTime        Started           { get; init; }
    /// <inheritdoc/>
    public required ExecutionStatus ExecutionStatus   { get; init; }
    /// <inheritdoc/>
    public          string?         Progress          { get; init; }
    /// <inheritdoc/>
    public          DateTime?       Finished          { get; init; }
    /// <inheritdoc/>
    public required int             JapServerProcessId { get; init; }
    /// <inheritdoc/>
    public required int             JobHostProcessId  { get; init; }
    /// <inheritdoc/>
    public          DateTime?       LastWriteTime     { get; init; }
    /// <inheritdoc/>
    public          string?         Host              { get; init; }
}
