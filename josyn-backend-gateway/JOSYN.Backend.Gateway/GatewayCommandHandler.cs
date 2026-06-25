using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Surface;
using BackendOutcome = JOSYN.Backend.JobRegistry.ArgumentChangeOutcome;
using JrpOutcome = JOSYN.Jrp.Surface.ArgumentChangeOutcome;

namespace JOSYN.Backend.Gateway;

/// <summary>
/// Platform-resident command handler for the JRP surface concern (ADR-031 DS-5, ADR-033 T-3).
/// </summary>
/// <remarks>
/// Bridges JRP write commands to the platform's backend store. Accepts <see cref="ChangeJobArgument"/>
/// from the JRP contract, delegates writes to <see cref="IJobArgumentWriter"/>, and maps the backend
/// result to <see cref="JrpOutcome"/> before returning — so no backend type crosses the JRP boundary.
/// <para>
/// At MVP-2 it is called in-process by the surface via <see cref="IGatewayCommandHandler"/>. In a
/// later increment it will be hosted inside a platform-resident EXE (Windows Service / daemon) and
/// reached by the surface over REST — with no change to the logic here (ADR-031 DS-2 swap guarantee).
/// </para>
/// <para>
/// It receives the full command envelope (actor, target, correlation) so those fields are available
/// when audit enforcement is added; they are not enforced here yet (ADR-031 DS-5, ADR-030 D-9/D-10).
/// </para>
/// </remarks>
public sealed class GatewayCommandHandler(string connectionString) : IGatewayCommandHandler
{
    /// <summary>
    /// Changes the content of an existing job argument record.
    /// </summary>
    /// <param name="command">The full JRP command envelope.</param>
    /// <returns>
    /// <see cref="JrpOutcome"/> with before/after content on success.
    /// A <c>[NotFound]</c> failure when the job or argument record is absent.
    /// </returns>
    public Result<JrpOutcome> HandleChangeJobArgument(ChangeJobArgument command)
    {
        // DS-5: write logic is platform-resident; must never live in the surface repo.
        IJobArgumentWriter writer = new SqlJobRegistry(connectionString);
        var backendResult = writer.ChangeArgument(command.JobName, command.ArgumentName, command.Content);

        if (!backendResult.Succeeded)
            return backendResult.ToResult<JrpOutcome>();

        return Result<JrpOutcome>.Success(MapOutcome(backendResult.Value));

        // ── helpers ──────────────────────────────────────────────────────────
        static JrpOutcome MapOutcome(BackendOutcome backend) => new()
        {
            JobName      = backend.JobName,
            ArgumentName = backend.ArgumentName,
            Before       = backend.Before,
            After        = backend.After
        };
    }
}
