namespace JOSYN.Backend.Contracts;

/// <summary>
/// Deployment and launch-protocol constants shared between <c>SessionBroker</c> and its spawner
/// (<c>SessionLauncher</c>).
/// </summary>
public static class SessionBrokerConstants
{
    /// <summary>Subdirectory name under <c>BackendRoot</c> that contains the SessionBroker executable.</summary>
    public const string FolderName = "SessionBroker";

    /// <summary>SessionBroker executable file name.</summary>
    public const string ExeName = "SessionBroker.exe";

    /// <summary>
    /// CLI mode token used to launch a new session via a temp-file handoff.
    /// Passed as the first argument when spawning SessionBroker in start mode:
    /// <c>SessionBroker.exe JOSYN-START @&lt;path&gt;</c>.
    /// </summary>
    public const string CliModeStart = "JOSYN-START";
}
