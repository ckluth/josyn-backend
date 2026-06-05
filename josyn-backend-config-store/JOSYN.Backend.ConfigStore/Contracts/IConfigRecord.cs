namespace JOSYN.Backend.ConfigStore;

/// <summary>
/// Represents a single configuration entry in the JOSYN config store.
/// </summary>
public interface IConfigRecord
{
    /// <summary>Surrogate primary key.</summary>
    int Id { get; init; }

    /// <summary>Configuration key. Unique within the store.</summary>
    string Key { get; init; }

    /// <summary>Configuration value.</summary>
    string Value { get; init; }
}
