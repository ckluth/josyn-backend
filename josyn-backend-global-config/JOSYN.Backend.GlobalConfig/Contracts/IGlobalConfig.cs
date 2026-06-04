namespace JOSYN.Backend.GlobalConfig;

/// <summary>
/// Runtime configuration contract for backend components.
/// </summary>
public interface IGlobalConfig
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
