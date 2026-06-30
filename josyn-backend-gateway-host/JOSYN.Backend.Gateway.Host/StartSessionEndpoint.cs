using JOSYN.Jrp.Launch;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <c>POST /v1/sessions</c>: validates a launch request and fires it asynchronously.
/// Node-specific execution verb — <c>Machine</c> must name this host (ADR-035 D-1, D-2).
/// Returns 202 Accepted: the GUID is allocated later inside SessionBroker.exe.
/// </summary>
internal sealed class StartSessionEndpoint(GatewayStartup startup) : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/v1/sessions", Handle)
            .Accepts<StartSessionRequest>("application/json")
            .Produces<StartSessionResponse>(202)
            .ProducesProblem(400)
            .ProducesProblem(500)
            .WithName("StartSession")
            .WithTags("Sessions");

    private IResult Handle(StartSessionRequest request)
    {
        var envResult = TargetValidation.ValidateEnvironment(request.Target, startup.Environment);
        if (!envResult.Succeeded) return envResult.ToHttpResult();

        // start-session is node-specific: reject if the request targets a different machine.
        var machineResult = TargetValidation.ValidateMachine(request.Target.Machine);
        if (!machineResult.Succeeded) return machineResult.ToHttpError();

        return new StartSessionHandler(startup.ConnectionString, startup.BackendRoot)
            .Handle(request)
            .ToHttpAccepted();
    }
}
