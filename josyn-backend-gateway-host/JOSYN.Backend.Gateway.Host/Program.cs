using JOSYN.Backend.BootstrapConfig;
using JOSYN.Jap.Contract;

namespace JOSYN.Backend.Gateway.Host;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var startup = LoadStartup();
        if (startup is null) return 1;

        var app = HostFactory.Create(args, startup);
        await app.RunAsync();
        return 0;

        // ── helpers ──────────────────────────────────────────────────────────
        static GatewayStartup? LoadStartup()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName);
            var loadResult = FileBootstrapConfig.Load(configPath);
            if (!loadResult.Succeeded)
            {
                Console.Error.WriteLine(
                    $"[JOSYN.Gateway] Bootstrap config could not be loaded: {loadResult.ErrorMessage}");
                return null;
            }

            var cfg = loadResult.Value;

            if (cfg.RuntimeEnvironment is null ||
                !Enum.TryParse<RuntimeEnvironment>(cfg.RuntimeEnvironment, ignoreCase: true, out var env))
            {
                Console.Error.WriteLine(
                    "[JOSYN.Gateway] 'RuntimeEnvironment' is missing or invalid in josyn.bootstrap.ini. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<RuntimeEnvironment>())}.");
                return null;
            }

            var listenUrl = cfg.GatewayListenUrl ?? "https://localhost:5001";
            return new GatewayStartup(cfg.SessionStoreConnectionString, cfg.BackendRoot, env, listenUrl);
        }
    }
}
