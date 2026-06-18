using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace JOSYN.Backend.Ticker;

/// <summary>
/// Owns the one-second timer and fires due targets on each tick.
/// Call <see cref="Start"/> once; call <see cref="Stop"/> on shutdown.
/// </summary>
internal static class TickerLoop
{
    // AutoReset = false: the timer is restarted at the end of each tick,
    // so a slow tick can never cause two callbacks to run concurrently.
    private static Timer?                    _timer;
    private static Dictionary<string, Process?> _state       = [];
    private static IReadOnlyList<TickerTarget>  _targets     = [];
    private static string                       _backendRoot = string.Empty;
    private static bool                         _interactive;

    internal static void Start(IReadOnlyList<TickerTarget> targets, string backendRoot, bool interactive)
    {
        _targets     = targets;
        _backendRoot = backendRoot;
        _interactive = interactive;
        _state       = targets.ToDictionary(t => t.Name, _ => (Process?)null);

        _timer = new Timer(TimeSpan.FromSeconds(1)) { AutoReset = false };
        _timer.Elapsed += OnTick;
        _timer.Start();
    }

    internal static void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        foreach (var p in _state.Values) p?.Dispose();
    }

    // ── tick ──────────────────────────────────────────────────────────────────

    private static void OnTick(object? sender, ElapsedEventArgs e)
    {
        var now     = DateTime.Now;
        var second  = now.Second;
        var running = SnapshotRunning(); // capture before any spawning

        var actions = _targets
            .Select(t => EvaluateTarget(t, second))
            .Where(a => a is not null)
            .ToList();

        if (_interactive && actions.Count > 0)
            PrintStatus(now, running, actions!);

        _timer?.Start(); // restart for next tick (AutoReset = false)
    }

    private static string[] SnapshotRunning() =>
        _state
            .Where(kv => IsStillRunning(kv.Value))
            .Select(kv => $"{kv.Key} (PID {kv.Value!.Id})")
            .ToArray();

    private static string? EvaluateTarget(TickerTarget target, int second)
    {
        if (!IsDue(target, second))
            return null;

        var last = _state[target.Name];

        if (IsStillRunning(last))
            return $"{target.Name,-20} skipped  — PID {last!.Id} still running";

        last?.Dispose();
        _state[target.Name] = null;

        try
        {
            var spawned = ProcessSpawner.Fire(target, _backendRoot, _interactive);
            _state[target.Name] = spawned;
            return $"{target.Name,-20} fired    — PID {spawned?.Id.ToString() ?? "?"}";
        }
        catch (Exception ex)
        {
            return $"{target.Name,-20} ERROR    — {ex.Message}";
        }
    }

    // ── schedule ──────────────────────────────────────────────────────────────

    private static bool IsDue(TickerTarget target, int second) =>
        second % target.Period == target.Offset % target.Period;

    private static bool IsStillRunning(Process? process)
    {
        if (process is null) return false;
        try   { return !process.HasExited; }
        catch { return false; }
    }

    // ── console output ────────────────────────────────────────────────────────

    private static void PrintStatus(DateTime now, string[] running, List<string> actions)
    {
        var runningInfo = running.Length > 0
            ? $"  running: {string.Join(", ", running)}"
            : string.Empty;

        Console.WriteLine($"[{now:HH:mm:ss}]{runningInfo}");
        foreach (var action in actions)
            Console.WriteLine($"  {action}");
    }
}
