namespace JOSYN.Backend.Ticker;

internal static class Program
{
    private static int Main()
    {
        var loadResult = BootstrapReader.Load();
        if (!loadResult.Succeeded)
            return FailWith($"Startup failed: {loadResult.ErrorMessage}");

        var (backendRoot, targets) = loadResult.Value;

        return Environment.UserInteractive
            ? ConsoleHost.Run(targets, backendRoot)
            : ServiceHost.Run(targets, backendRoot);

        // ── helpers ───────────────────────────────────────────────────────────────
        static int FailWith(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}

