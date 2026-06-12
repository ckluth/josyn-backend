namespace JOSYN.Backend.SessionLauncherContract;

/// <summary>
/// Describes a request to start a new job session.
/// Produced by orchestrators (via <c>JOSYN.Backend.SessionLauncher</c>)
/// and consumed by <c>JAPServer</c> in <c>JOSYN-START</c> mode.
/// </summary>
public record SessionStartRequest
{
    /// <summary>The registered job type name.</summary>
    public string JobTypeName         { get; init; } = string.Empty;

    /// <summary>Serialized job arguments (INI format).</summary>
    public string Arguments           { get; init; } = string.Empty;

    /// <summary>
    /// Windows account name under which <c>job.exe</c> should be spawned.
    /// Resolved from <c>JobRegistry</c> by <c>SessionLauncher</c> — not supplied by the caller.
    /// </summary>
    public string TechnicalUserName   { get; init; } = string.Empty;

    /// <summary>Windows user name of the caller process.</summary>
    public string CallerUser          { get; init; } = string.Empty;

    /// <summary>Windows domain name of the caller process.</summary>
    public string CallerDomain        { get; init; } = string.Empty;

    /// <summary>Friendly name of the calling application.</summary>
    public string CallerApplication   { get; init; } = string.Empty;

    /// <summary>Machine name of the caller process.</summary>
    public string CallerMachine       { get; init; } = string.Empty;
}
