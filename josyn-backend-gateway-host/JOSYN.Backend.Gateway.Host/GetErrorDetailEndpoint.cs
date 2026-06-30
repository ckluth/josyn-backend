using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>GET /v1/errors/{errorUid}</c>: the full detail of one stored error by its UID.
/// Environment-scoped data verb — any Gateway in the environment answers identically (ADR-035 D-1).
/// </summary>
internal sealed class GetErrorDetailEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/v1/errors/{errorUid}", Handle)
            .Produces<ErrorDetail>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .WithName("GetErrorDetail")
            .WithTags("Errors");

    private IResult Handle(Guid errorUid, string? environment, string? machine)
    {
        var targetResult = TargetValidation.ParseAndValidate(environment, machine, startup.Environment);
        if (!targetResult.Succeeded) return targetResult.ToHttpResult();

        return new GetErrorDetailHandler(startup.ConnectionString)
            .Handle(new GetErrorDetail { Target = targetResult.Value, ErrorUid = errorUid })
            .ToHttpResult();
    }
}
