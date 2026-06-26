namespace JOSYN.Backend.Gateway.Host;

/// <summary>One JRP verb = one endpoint implementation (ADR-034 D-2).</summary>
internal interface IEndpoint
{
    /// <summary>Registers this endpoint's route(s) on <paramref name="app"/>.</summary>
    void Map(IEndpointRouteBuilder app);
}
