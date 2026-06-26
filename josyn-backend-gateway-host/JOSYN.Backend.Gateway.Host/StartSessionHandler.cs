using JOSYN.Backend.Contracts;
using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Launch;
using Launcher = JOSYN.Backend.SessionLauncher.SessionLauncher;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <see cref="StartSessionRequest"/>: validates the request, maps it to a
/// <see cref="SessionLaunchRequest"/>, and fires the session asynchronously via
/// <see cref="SessionLauncher.LaunchSession"/>.
/// </summary>
/// <remarks>
/// The session GUID is allocated inside <c>SessionBroker.exe</c> after it starts (ADR-017B-01
/// step 3). The Gateway does not and cannot know the GUID at the time this acknowledgement is
/// returned. <see cref="StartSessionResponse"/> is therefore a fire-and-forget acknowledgement —
/// no GUID is included.
/// </remarks>
internal sealed class StartSessionHandler(string connectionString, string backendRoot)
{
    internal Result<StartSessionResponse> Handle(StartSessionRequest request)
    {
        IJobRegistry registry = new SqlJobRegistry(connectionString);
        var launchRequest     = MapRequest(request);
        var launchResult      = Launcher.LaunchSession(launchRequest, backendRoot, registry);

        if (!launchResult.Succeeded)
            return launchResult.ToResult<StartSessionResponse>();

        return Result<StartSessionResponse>.Success(new StartSessionResponse());

        // ── helpers ──────────────────────────────────────────────────────────
        // The Actor field carries a caller identity string (e.g. "DOMAIN\user" or just a
        // user name). Split on '\' so SessionBroker receives the user and domain separately.
        static SessionLaunchRequest MapRequest(StartSessionRequest r)
        {
            var actorParts    = r.Actor.Split('\\', 2);
            var callerUser    = actorParts.Length == 2 ? actorParts[1] : r.Actor;
            var callerDomain  = actorParts.Length == 2 ? actorParts[0] : string.Empty;

            return new SessionLaunchRequest
            {
                JobTypeName       = r.JobName,
                Arguments         = r.ArgumentsBase64,
                CallerUser        = callerUser,
                CallerDomain      = callerDomain,
                CallerApplication = "JOSYN.Backend.Gateway.Host",
                CallerMachine     = Environment.MachineName,
                Interactive       = false
            };
        }
    }
}
