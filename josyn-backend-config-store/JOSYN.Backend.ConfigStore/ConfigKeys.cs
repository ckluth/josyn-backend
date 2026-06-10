namespace JOSYN.Backend.ConfigStore;

/// <summary>
/// Well-known keys for the JOSYN configuration store (<see cref="IConfigStore"/>).
/// Use these constants instead of raw strings to avoid typos and to make key usage traceable.
/// </summary>
/// <remarks>
/// The catalogue of all keys and their valid values is documented in
/// <c>josyn-platform/architecture/storage.md</c> under <em>ConfigStore Key Catalogue</em>.
/// </remarks>
public static class ConfigKeys
{
    /// <summary>
    /// The runtime environment of this JOSYN installation.
    /// Valid values: <c>DEV</c>, <c>INT</c>, <c>PROD</c> (names of <c>RuntimeEnvironment</c> enum in <c>JOSYN.Jap.Contract</c>).
    /// Written by: installation/setup. Read by: <c>JAPServer.GetEnvironment()</c>.
    /// </summary>
    public const string RuntimeEnvironment = "RuntimeEnvironment";
}
