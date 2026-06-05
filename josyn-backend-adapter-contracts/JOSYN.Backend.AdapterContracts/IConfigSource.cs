using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.AdapterContracts;

/// <summary>
/// Provides read access to JOSYN runtime configuration values.
/// <para>
/// The built-in implementation (<c>SqlConfigSource</c>) reads from the
/// <c>josyn.ConfigStore</c> table. A company-specific adapter may replace it
/// by implementing this interface and registering it via the bootstrap config
/// (see ADR-009).
/// </para>
/// </summary>
public interface IConfigSource
{
    /// <summary>
    /// Returns the value associated with <paramref name="key"/>.
    /// Fails if the key does not exist or the source is unavailable.
    /// </summary>
    Result<string> GetValue(string key);
}
