namespace JOSYN.Backend.SurfaceAgent;

/// <summary>
/// The backend-side representation of a "change an existing job argument record" command.
/// Carries the full envelope required from MVP-2 onward (ADR-031 DS-5): actor identity, target, and
/// a correlation ID for the audit trail. Enforcement of actor/audit is deferred; the fields exist
/// from the first write so they are not retrofitted later.
/// </summary>
public sealed record ChangeJobArgumentCommand
{
    /// <summary>The user or service principal that issued the command.</summary>
    public required string Actor { get; init; }

    /// <summary>Target environment (matches the platform installation — ADR-010).</summary>
    public required string Environment { get; init; }

    /// <summary>Target machine identifier.</summary>
    public required string Machine { get; init; }

    /// <summary>Correlation ID for tracing this command through logs and (future) audit records.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Registry name of the job whose argument is to be changed.</summary>
    public required string JobName { get; init; }

    /// <summary>Name of the argument record to change.</summary>
    public required string ArgumentName { get; init; }

    /// <summary>New content for the argument record.</summary>
    public required string Content { get; init; }
}
