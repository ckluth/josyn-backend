using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SurfaceAgent;

/// <summary>
/// The platform-resident command logic for the josyn-surface human window (ADR-031 DS-5).
/// </summary>
/// <remarks>
/// This class is the long-lived home of all surface write operations. At MVP-2 it is invoked
/// in-process by the surface via a direct method call. In a later increment it will be hosted inside
/// a platform-resident EXE (Windows Service / daemon) and reached by the surface over REST — with no
/// change to the logic here (ADR-031 DS-2 swap guarantee applied to the write side).
/// <para>
/// It receives the full command envelope (actor, target, correlation) from the surface so that those
/// fields are available when audit enforcement is added; they are not enforced here yet (ADR-031 DS-5,
/// ADR-030 D-9/D-10 — enforcement deferred).
/// </para>
/// </remarks>
public sealed class SurfaceCommandHandler(string connectionString) : ISurfaceCommandHandler
{
    /// <summary>
    /// Changes the content of an existing job argument record.
    /// </summary>
    /// <param name="command">The full command envelope.</param>
    /// <returns>
    /// <see cref="ArgumentChangeOutcome"/> with before/after content on success.
    /// A <c>[NotFound]</c> failure when the job or argument record is absent.
    /// </returns>
    public Result<ArgumentChangeOutcome> HandleChangeJobArgument(ChangeJobArgumentCommand command)
    {
        // Placement is the point (DS-5): the write logic is platform-resident even though the
        // transport (network call) is deferred. The SqlJobRegistry write touches the platform's
        // own database on its own machine — this handler must never live in the surface repo.
        IJobArgumentWriter writer = new SqlJobRegistry(connectionString);
        return writer.ChangeArgument(command.JobName, command.ArgumentName, command.Content);
    }
}
