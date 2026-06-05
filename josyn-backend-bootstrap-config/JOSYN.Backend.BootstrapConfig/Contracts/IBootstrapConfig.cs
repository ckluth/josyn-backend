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
    /// <summary>ADO.NET connection string for the session store database.</summary>
    string SessionStoreConnectionString { get; }

    /// <summary>Absolute path to the <c>JOSYN.Jap.JAPServer.exe</c> binary.</summary>
    string JapServerExePath { get; }

    /// <summary>
    /// Absolute path to the directory that contains job executables.
    /// Convention: each job exe is named <c>{JobTypeName}.exe</c>.
    /// </summary>
    string JobRepositoryRoot { get; }
}
