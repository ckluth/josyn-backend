namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// The single, explicit list of JRP endpoints (ADR-034 D-2: static-first, no DI container, no
/// reflection scan). Every JRP verb is one <see cref="IEndpoint"/>; they are enumerated here by hand
/// so the full HTTP surface is readable — and greppable — from one file. Adding a verb means adding
/// one line to the collection inside <see cref="MapEndpoints"/>.
/// </summary>
internal static class EndpointRegistry
{
    /// <summary>
    /// Instantiates every JRP endpoint with the shared <paramref name="startup"/> config and maps
    /// each one onto the application's route table.
    /// </summary>
    internal static WebApplication MapEndpoints(this WebApplication app, GatewayStartup startup)
    {
        IReadOnlyList<IEndpoint> all =
        [
            new GetRegisteredJobsEndpoint(startup),
            new GetRecentSessionsEndpoint(startup),
            new GetJobArgumentsEndpoint(startup),
            new GetJobScheduleEndpoint(startup),
            new GetErrorDetailEndpoint(startup),
            new ChangeJobArgumentEndpoint(startup),
            new StartSessionEndpoint(startup),
        ];

        foreach (var endpoint in all)
            endpoint.Map(app);

        return app;
    }
}
