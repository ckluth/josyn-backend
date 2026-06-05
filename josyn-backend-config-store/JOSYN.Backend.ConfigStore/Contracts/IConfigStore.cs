using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.ConfigStore;

/// <summary>
/// Read/write access to the JOSYN configuration store.
/// </summary>
public interface IConfigStore
{
    /// <summary>
    /// Returns the value associated with <paramref name="key"/>.
    /// Fails if the key does not exist.
    /// </summary>
    Result<string> GetValue(string key);

    /// <summary>
    /// Inserts or updates the entry for <paramref name="key"/>.
    /// </summary>
    Result SetValue(string key, string value);
}
