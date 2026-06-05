namespace JOSYN.Backend.ConfigStore;

/// <inheritdoc cref="IConfigRecord"/>
public sealed record ConfigRecord : IConfigRecord
{
    /// <inheritdoc/>
    public int Id { get; init; }

    /// <inheritdoc/>
    public string Key { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Value { get; init; } = string.Empty;
}
