using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>GET /v1/jobs/{jobName}/arguments</c>: all argument records for one registered job.
/// Environment-scoped data verb — any Gateway in the environment answers identically (ADR-035 D-1).
/// </summary>
internal sealed class GetJobArgumentsEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/v1/jobs/{jobName}/arguments", Handle)
            .Produces<JobArguments>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .WithName("GetJobArguments")
            .WithTags("Jobs");

    private IResult Handle(string jobName, string? environment, string? machine)
    {
        var targetResult = TargetValidation.ParseAndValidate(environment, machine, startup.Environment);
        if (!targetResult.Succeeded) return targetResult.ToHttpResult();

        return new GetJobArgumentsHandler(startup.ConnectionString)
            .Handle(new GetJobArguments { Target = targetResult.Value, JobName = jobName })
            .ToHttpResult();
    }
}
