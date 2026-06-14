namespace JOSYN.Backend.Contracts;

/// <summary>
/// Describes a request to launch a new job session.
/// Produced by orchestrators and passed to <c>ISessionLauncher</c>.
/// Does not include <c>TechnicalUserName</c> — that is resolved from <c>JobRegistry</c>
/// by the launcher and placed into <see cref="SessionStartSpec"/>.
/// </summary>
public record SessionLaunchRequest
{
    /// <summary>The registered job type name.</summary>
    public string JobTypeName         { get; init; } = string.Empty;

    /// <summary>
    /// Job arguments file content, <b>base64-encoded</b> (ADR-007 §3a).
    /// Orchestrators must encode before populating this field.
    /// JAPServer decodes before storing in <c>SessionStore</c>.
    /// <c>job.exe</c> always receives the decoded plain-text INI.
    /// </summary>
    public string Arguments           { get; init; } = string.Empty;

    /// <summary>Windows user name of the caller process.</summary>
    public string CallerUser          { get; init; } = string.Empty;

    /// <summary>Windows domain name of the caller process.</summary>
    public string CallerDomain        { get; init; } = string.Empty;

    /// <summary>Friendly name of the calling application.</summary>
    public string CallerApplication   { get; init; } = string.Empty;

    /// <summary>Machine name of the caller process.</summary>
    public string CallerMachine       { get; init; } = string.Empty;
}
