namespace JOSYN.Backend.BootstrapConfig;

/// <summary>
/// Bootstrap configuration contract for <c>josyn-backend</c>.
/// Provides all values that must be known before the backend can start:
/// database connection, executable paths, and deployment-local paths.
/// </summary>
/// <remarks>
/// The active implementation is determined at startup. The built-in implementation
/// reads from <c>josyn.bootstrap.ini</c> (see <see cref="FileBootstrapConfig"/>).
/// HAEVG-specific or other deployment-specific implementations are loaded at runtime
/// via the adapter mechanism defined in ADR-009.
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
    /// Fully qualified type name of the <c>IConfigSource</c> adapter to load at startup,
    /// in the format <c>TypeName, AssemblyName</c>.
    /// When <see langword="null"/>, the built-in <c>SqlConfigSource</c> is used.
    /// The adapter assembly must be present in the <c>Adapters/</c> subfolder next to the backend executable.
    /// </summary>
    string? ConfigSourceType { get; }
}
