using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>GET /v1/sessions</c>: the most recent sessions, newest first.
/// Environment-scoped data verb — any Gateway in the environment answers identically (ADR-035 D-1).
/// </summary>
internal sealed class GetRecentSessionsEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/v1/sessions", Handle)
            .Produces<IReadOnlyList<SessionSummary>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500)
            .WithName("GetRecentSessions")
            .WithTags("Sessions");

    private IResult Handle(string? environment, string? machine, int? maxCount)
    {
        var targetResult = TargetValidation.ParseAndValidate(environment, machine, startup.Environment);
        if (!targetResult.Succeeded) return targetResult.ToHttpResult();

        if (maxCount is null or <= 0)
            return Results.Problem(detail: $"Query parameter 'maxCount' must be a positive integer.", statusCode: 400);

        return new GetRecentSessionsHandler(startup.ConnectionString)
            .Handle(new GetRecentSessions { Target = targetResult.Value, MaxCount = maxCount.Value })
            .ToHttpResult();
    }
}
