#pragma warning disable IDE0130
namespace JOSYN.Backend.BootstrapConfig;
#pragma warning restore IDE0130

/// <summary>
/// Bootstrap configuration contract for <c>josyn-backend</c>.
/// Provides all values that must be known before the backend can start:
/// database connection, executable paths, and deployment-local paths.
/// </summary>
/// <remarks>
/// The active implementation is determined at startup. The built-in implementation
/// reads from <c>josyn.bootstrap.ini</c> (see <see cref="FileBootstrapConfig"/>).
/// </remarks>
public interface IBootstrapConfig
{
    /// <summary>
    /// Absolute path to the backend installation root — the directory containing
    /// <c>josyn.bootstrap.ini</c>. All other deployment paths derive from this root
    /// by convention (see ADR-012).
    /// </summary>
    string BackendRoot { get; }

    /// <summary>ADO.NET connection string for the session store database.</summary>
    string SessionStoreConnectionString { get; }

    /// <summary>
    /// The runtime environment this installation serves (e.g. <c>"DEV"</c>, <c>"INT"</c>,
    /// <c>"PROD"</c>). Read from the root key <c>RuntimeEnvironment</c> in
    /// <c>josyn.bootstrap.ini</c>.
    /// <para>
    /// Optional — not all hosts require this value. The Gateway host validates it is present
    /// and parseable at startup and fails fast if it is missing.
    /// </para>
    /// </summary>
    string? RuntimeEnvironment { get; }

    /// <summary>
    /// The URL the JRP Gateway host listens on (e.g. <c>"https://localhost:5001"</c>).
    /// Read from the root key <c>GatewayListenUrl</c> in <c>josyn.bootstrap.ini</c>.
    /// Optional — used only by the Gateway host; ignored by all other hosts.
    /// </summary>
    string? GatewayListenUrl { get; }

    /// <summary>
    /// Adapter EXE registrations, keyed by concern name.
    /// Populated from the <c>[Adapters]</c> section of <c>josyn.bootstrap.ini</c>.
    /// Each entry maps a concern (e.g. <c>"IdentityAdapter"</c>) to an EXE filename
    /// within the <c>Adapters/</c> subfolder next to <c>SessionBroker.exe</c>
    /// (e.g. <c>"IdentityAdapter.exe"</c>).
    /// Empty when the <c>[Adapters]</c> section is absent. Validation of required
    /// adapters is the caller's responsibility.
    /// </summary>
    IReadOnlyDictionary<string, string> Adapters { get; }
}
