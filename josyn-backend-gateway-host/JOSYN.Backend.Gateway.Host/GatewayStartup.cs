using JOSYN.Jap.Contract;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Startup configuration assembled from <c>josyn.bootstrap.ini</c> at program entry.
/// Threaded through <see cref="HostFactory"/> to all endpoint instances so every handler
/// receives its dependencies without a DI container (ADR-034 D-2: static-first, no DI).
/// </summary>
internal sealed record GatewayStartup(
    string ConnectionString,
    string BackendRoot,
    RuntimeEnvironment Environment,
    string ListenUrl);
