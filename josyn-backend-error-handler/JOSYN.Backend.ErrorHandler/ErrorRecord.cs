namespace JOSYN.Backend.ErrorHandler;

/// <inheritdoc cref="IErrorRecord"/>
public sealed record ErrorRecord : IErrorRecord
{
    /// <inheritdoc/>
    public required Guid            UID              { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset  OccurredAt       { get; init; }

    /// <inheritdoc/>
    public required string          Causer           { get; init; }

    /// <inheritdoc/>
    public required string          Message          { get; init; }

    /// <inheritdoc/>
    public string?         CallStack        { get; init; }

    /// <inheritdoc/>
    public string?         ExceptionDetails { get; init; }

    /// <inheritdoc/>
    public string?         JobName          { get; init; }

    /// <inheritdoc/>
    public Guid?           SessionGuid      { get; init; }
}
