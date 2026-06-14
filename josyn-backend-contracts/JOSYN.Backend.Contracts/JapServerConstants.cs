namespace JOSYN.Backend.Contracts;

/// <summary>
/// Deployment and launch-protocol constants shared between JAPServer and its spawner
/// (<c>SessionLauncher</c>).
/// </summary>
public static class JapServerConstants
{
    /// <summary>Subdirectory name under <c>BackendRoot</c> that contains the JAPServer executable.</summary>
    public const string FolderName = "JAPServer";

    /// <summary>JAPServer executable file name.</summary>
    public const string ExeName = "JOSYN.Jap.JAPServer.exe";

    /// <summary>
    /// CLI mode token used to launch a new session via a temp-file handoff.
    /// Passed as the first argument when spawning JAPServer in start mode:
    /// <c>JAPServer.exe JOSYN-START @&lt;path&gt;</c>.
    /// </summary>
    public const string CliModeStart = "JOSYN-START";
}
