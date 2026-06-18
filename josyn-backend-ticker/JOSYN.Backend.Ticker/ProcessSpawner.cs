using System.Diagnostics;

namespace JOSYN.Backend.Ticker;

/// <summary>
/// Process launcher for Ticker targets.
/// Resolves the EXE path using the Orchestrators convention (ADR-024):
/// <c>BackendRoot\Orchestrators\&lt;Name&gt;\&lt;ExeName&gt;</c>.
/// </summary>
internal static class ProcessSpawner
{
    private const string OrchestratorsFolderName = "Orchestrators";

    /// <summary>
    /// Spawns <paramref name="target"/> as an independent process and returns it.
    /// The caller is responsible for tracking and disposing the returned <see cref="Process"/>.
    /// </summary>
    /// <param name="interactive">
    /// <c>true</c> — the process gets its own console window (console-mode Ticker);
    /// <c>false</c> — headless, no window (service-mode Ticker).
    /// </param>
    internal static Process? Fire(TickerTarget target, string backendRoot, bool interactive)
    {
        var exePath = Path.GetFullPath(
            Path.Combine(backendRoot, OrchestratorsFolderName, target.Name, target.ExeName));

        var info = new ProcessStartInfo(exePath)
        {
            WorkingDirectory = Path.GetDirectoryName(exePath) ?? backendRoot,
            UseShellExecute  = interactive,   // true → OS shell opens exe → new console window
            CreateNoWindow   = !interactive,
        };

        return Process.Start(info);
    }
}
