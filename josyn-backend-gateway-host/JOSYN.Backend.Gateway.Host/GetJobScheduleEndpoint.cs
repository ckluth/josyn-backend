using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>GET /v1/jobs/{jobName}/schedule</c>: the schedule and all its entries for one job.
/// Environment-scoped data verb — any Gateway in the environment answers identically (ADR-035 D-1).
/// </summary>
internal sealed class GetJobScheduleEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/v1/jobs/{jobName}/schedule", Handle)
            .Produces<JobSchedule>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .WithName("GetJobSchedule")
            .WithTags("Jobs");

    private IResult Handle(string jobName, string? environment, string? machine)
    {
        var targetResult = TargetValidation.ParseAndValidate(environment, machine, startup.Environment);
        if (!targetResult.Succeeded) return targetResult.ToHttpResult();

        return new GetJobScheduleHandler(startup.ConnectionString)
            .Handle(new GetJobSchedule { Target = targetResult.Value, JobName = jobName })
            .ToHttpResult();
    }
}
