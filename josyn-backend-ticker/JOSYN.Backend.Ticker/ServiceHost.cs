using System.ServiceProcess;

namespace JOSYN.Backend.Ticker;

/// <summary>
/// Windows Service host for the Ticker.
/// Delegates start/stop to <see cref="TickerLoop"/>; the SCM manages the process lifetime.
/// </summary>
internal sealed class ServiceHost : ServiceBase
{
    private readonly IReadOnlyList<TickerTarget> _targets;
    private readonly string _backendRoot;

    private ServiceHost(IReadOnlyList<TickerTarget> targets, string backendRoot)
    {
        ServiceName  = "JOSYN.Backend.Ticker";
        _targets     = targets;
        _backendRoot = backendRoot;
    }

    internal static int Run(IReadOnlyList<TickerTarget> targets, string backendRoot)
    {
        Run(new ServiceHost(targets, backendRoot));
        return 0;
    }

    protected override void OnStart(string[] args) =>
        TickerLoop.Start(_targets, _backendRoot, interactive: false);

    protected override void OnStop() =>
        TickerLoop.Stop();
}
