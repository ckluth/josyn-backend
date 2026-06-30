using JOSYN.Backend.Gateway;
using JOSYN.Jrp.Surface;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>PUT /v1/jobs/{jobName}/arguments/{argumentName}</c>: changes the content of an
/// existing argument record. Environment-scoped write verb (ADR-035 D-1).
/// </summary>
/// <remarks>
/// The <see cref="ChangeJobArgument"/> command envelope is the authoritative source for all
/// fields; the route parameters <c>{jobName}</c> and <c>{argumentName}</c> are validated against
/// it so URL and body are always consistent.
/// </remarks>
internal sealed class ChangeJobArgumentEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/v1/jobs/{jobName}/arguments/{argumentName}", Handle)
            .Accepts<ChangeJobArgument>("application/json")
            .Produces<ArgumentChangeOutcome>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .WithName("ChangeJobArgument")
            .WithTags("Jobs");

    private IResult Handle(
        string jobName, string argumentName, ChangeJobArgument command)
    {
        // Route params must match the command envelope — catches misrouted requests early.
        if (!string.Equals(jobName, command.JobName, StringComparison.Ordinal))
            return Results.Problem(
                detail: $"Route jobName '{jobName}' does not match command JobName '{command.JobName}'.",
                statusCode: 400);
        if (!string.Equals(argumentName, command.ArgumentName, StringComparison.Ordinal))
            return Results.Problem(
                detail: $"Route argumentName '{argumentName}' does not match command ArgumentName '{command.ArgumentName}'.",
                statusCode: 400);

        var envResult = TargetValidation.ValidateEnvironment(command.Target, startup.Environment);
        if (!envResult.Succeeded) return envResult.ToHttpResult();

        return new GatewayCommandHandler(startup.ConnectionString)
            .HandleChangeJobArgument(command)
            .ToHttpResult();
    }
}
