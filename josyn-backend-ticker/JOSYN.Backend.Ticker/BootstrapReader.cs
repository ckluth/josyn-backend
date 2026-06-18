using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.Ticker;

/// <summary>
/// Reads the Ticker's bootstrap values from <c>josyn.bootstrap.ini</c>.
/// The Ticker only needs BackendRoot and the <c>[Ticker-Targets]</c> section —
/// it does not require a database connection string and therefore does not
/// use the full <c>FileBootstrapConfig</c>.
/// </summary>
internal static class BootstrapReader
{
    private const string IniFileName    = "josyn.bootstrap.ini";
    private const string TargetsSection = "Ticker-Targets";

    /// <summary>
    /// Loads the bootstrap file from one level above the Ticker EXE
    /// (<c>$BackendRoot\Ticker\JOSYN.Backend.Ticker.exe</c> → <c>$BackendRoot\josyn.bootstrap.ini</c>)
    /// and returns BackendRoot together with the parsed target list.
    /// </summary>
    internal static Result<(string BackendRoot, IReadOnlyList<TickerTarget> Targets)> Load()
    {
        var iniPath = ResolveIniPath();
        return LoadFromPath(iniPath);

        // ── helpers ───────────────────────────────────────────────────────────────
        // Convention: Ticker lives at $BackendRoot\Orchestrators\Ticker\.
        // josyn.bootstrap.ini is at $BackendRoot\ — two levels up.
        static string ResolveIniPath() =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", IniFileName));

        static Result<(string, IReadOnlyList<TickerTarget>)> LoadFromPath(string path)
        {
            if (!File.Exists(path))
                return Result.Error($"Bootstrap configuration file not found: '{path}'");

            var backendRoot = Path.GetDirectoryName(path);
            if (backendRoot is null)
                return Result.Error($"Could not derive BackendRoot from path: '{path}'");

            var raw = File.ReadAllText(path);
            var parsed = IniDictionarySerializer.Deserialize(raw);
            if (!parsed.Succeeded)
                return Result<(string, IReadOnlyList<TickerTarget>)>.Propagate(
                    parsed.ToResult<(string, IReadOnlyList<TickerTarget>)>());

            if (!parsed.Value.TryGetValue(TargetsSection, out var section) || section.Count == 0)
                return Result.Error($"Section '[{TargetsSection}]' is missing or empty in '{path}'.");

            var targetsResult = ParseTargets(section, path);
            if (!targetsResult.Succeeded)
                return Result<(string, IReadOnlyList<TickerTarget>)>.Propagate(
                    targetsResult.ToResult<(string, IReadOnlyList<TickerTarget>)>());

            return (backendRoot, targetsResult.Value);
        }
    }

    /// <summary>
    /// Parses target entries from the raw INI section dictionary.
    /// Each entry value must follow the format: <c>&lt;ExeName&gt; | &lt;offset&gt;, &lt;period&gt;</c>.
    /// offset = second within a minute (0–59); period = interval in seconds (1–60).
    /// </summary>
    private static Result<IReadOnlyList<TickerTarget>> ParseTargets(
        Dictionary<string, string> section, string sourcePath)
    {
        var targets = new List<TickerTarget>(section.Count);

        foreach (var (name, rawValue) in section)
        {
            var parsed = ParseEntry(name, rawValue, sourcePath);
            if (!parsed.Succeeded)
                return Result<IReadOnlyList<TickerTarget>>.Propagate(
                    parsed.ToResult<IReadOnlyList<TickerTarget>>());

            targets.Add(parsed.Value);
        }

        return targets;

        // ── helpers ───────────────────────────────────────────────────────────────
        static Result<TickerTarget> ParseEntry(string name, string rawValue, string sourcePath)
        {
            // Expected format: "TimeScheduler.exe | 0, 30"
            var parts = rawValue.Split('|', 2);
            if (parts.Length != 2)
                return Result.Error(
                    $"[{TargetsSection}] entry '{name}' in '{sourcePath}' is malformed. " +
                    $"Expected: '<ExeName> | <offset>, <period>'. Got: '{rawValue}'");

            var exeName     = parts[0].Trim();
            var schedulePart = parts[1].Trim();

            var schedParts = schedulePart.Split(',', 2);
            if (schedParts.Length != 2)
                return Result.Error(
                    $"[{TargetsSection}] entry '{name}' — schedule part is malformed. " +
                    $"Expected: '<offset>, <period>'. Got: '{schedulePart}'");

            if (!int.TryParse(schedParts[0].Trim(), out var offset) || offset < 0 || offset > 59)
                return Result.Error(
                    $"[{TargetsSection}] entry '{name}' — offset must be 0–59 (second within a minute). Got: '{schedParts[0].Trim()}'");

            if (!int.TryParse(schedParts[1].Trim(), out var period) || period < 1 || period > 60)
                return Result.Error(
                    $"[{TargetsSection}] entry '{name}' — period must be 1–60 (seconds). Got: '{schedParts[1].Trim()}'");

            return new TickerTarget(name, exeName, offset, period);
        }
    }
}
