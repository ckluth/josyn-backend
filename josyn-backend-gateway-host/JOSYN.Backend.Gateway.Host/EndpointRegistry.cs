namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// The single, explicit list of JRP endpoints (ADR-034 D-2: static-first, no DI container, no
/// reflection scan). Every JRP verb is one <see cref="IEndpoint"/>; they are enumerated here by hand
/// so the full HTTP surface is readable — and greppable — from one file. Adding a verb means adding
/// one line to <see cref="All"/>.
/// </summary>
internal static class EndpointRegistry
{
    /// <summary>
    /// The complete set of endpoints the host serves. G-5 populates this with the 5 read endpoints,
    /// <c>ChangeJobArgument</c>, and <c>start-session</c> — e.g. <c>new GetRegisteredJobsEndpoint()</c>.
    /// </summary>
    private static IReadOnlyList<IEndpoint> All { get; } =
    [
        // G-5: explicit endpoint instances go here, one per JRP verb.
    ];

    /// <summary>Maps every endpoint in <see cref="All"/> onto the application's route table.</summary>
    internal static WebApplication MapEndpoints(this WebApplication app)
    {
        foreach (var endpoint in All)
            endpoint.Map(app);

        return app;
    }
}
