using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>GET /v1/jobs</c>: all registered jobs as a lightweight discovery listing.
/// Environment-scoped data verb — any Gateway in the environment answers identically (ADR-035 D-1).
/// </summary>
internal sealed class GetRegisteredJobsEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/v1/jobs", Handle)
            .Produces<IReadOnlyList<RegisteredJobSummary>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500)
            .WithName("GetRegisteredJobs")
            .WithTags("Jobs");

    private IResult Handle(string? environment, string? machine)
    {
        var targetResult = TargetValidation.ParseAndValidate(environment, machine, startup.Environment);
        if (!targetResult.Succeeded) return targetResult.ToHttpResult();

        return new GetRegisteredJobsHandler(startup.ConnectionString)
            .Handle(new GetRegisteredJobs { Target = targetResult.Value })
            .ToHttpResult();
    }
}
