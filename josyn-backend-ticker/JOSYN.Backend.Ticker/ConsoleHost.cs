namespace JOSYN.Backend.Ticker;

/// <summary>
/// Runs the Ticker in console mode: prints a status line every tick and exits on any keypress.
/// </summary>
internal static class ConsoleHost
{
    internal static int Run(IReadOnlyList<TickerTarget> targets, string backendRoot)
    {
        Console.WriteLine("JOSYN Ticker — console mode.");
        Console.WriteLine($"BackendRoot : {backendRoot}");
        Console.WriteLine($"Targets     : {string.Join(", ", targets.Select(t => $"{t.Name} (offset={t.Offset}s, period={t.Period}s)"))}");
        Console.WriteLine();
        Console.WriteLine("Each target fires in its own console window.");
        Console.WriteLine("Fires are printed as they occur; silent ticks are omitted.");
        Console.WriteLine("Polling... press any key to quit.");
        Console.WriteLine();

        TickerLoop.Start(targets, backendRoot, interactive: true);
        Console.ReadKey(intercept: true);
        TickerLoop.Stop();

        Console.WriteLine();
        Console.WriteLine("Ticker stopped.");
        return 0;
    }
}
