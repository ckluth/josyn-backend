using JOSYN.Backend.BootstrapConfig;
using JOSYN.Commons.Log;
using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using System.Diagnostics;

namespace JOSYN.Jap.JAPServer;

internal static partial class Host
{
    private const string AdaptersFolder          = "Adapters";
    private const string AdapterCliToken         = "JOSYN-ADAPTER";
    internal const string IdentityAdapterConcern = "IdentityAdapter";

    /// <summary>
    /// Spawns all adapter EXEs declared in <paramref name="config"/> and establishes
    /// a JIP connection to each. Returns a ready <see cref="AdapterManager"/> on success,
    /// or a failed <see cref="Result{T}"/> if any adapter is missing or fails to connect.
    /// Hard failure — caller must not proceed if this returns a failure.
    /// </summary>
    internal static async Task<Result<AdapterManager>> SpawnAdapters(IBootstrapConfig config)
    {
        var adaptersDir = Path.Combine(AppContext.BaseDirectory, AdaptersFolder);
        var manager     = new AdapterManager();

        foreach (var (concernName, exeFileName) in config.Adapters)
        {
            var spawn = await SpawnAdapter(concernName, exeFileName, adaptersDir);
            if (!spawn.Succeeded)
            {
                manager.Dispose();
                return Result<AdapterManager>.Propagate(spawn.ToResult<AdapterManager>());
            }
            manager.Add(spawn.Value);
            LocalLog.WriteInfo($"Adapter '{concernName}' gestartet (PID {spawn.Value.Process.Id}).");
        }

        return Result<AdapterManager>.Success(manager);
    }

    private static async Task<Result<AdapterProcess>> SpawnAdapter(
        string concernName, string exeFileName, string adaptersDir)
    {
        var exePath = Path.Combine(adaptersDir, exeFileName);
        if (!File.Exists(exePath))
            return Result.Error(
                $"Adapter-EXE für '{concernName}' nicht gefunden: '{exePath}'. " +
                $"Stelle sicher, dass die Datei im '{AdaptersFolder}/'-Ordner liegt.");

        var sessionGuid = Guid.NewGuid();
        var arguments   = $"{AdapterCliToken} {sessionGuid}";

        Process? process;
        try
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName        = exePath,
                Arguments       = arguments,
                UseShellExecute = false,
                CreateNoWindow  = false,
            });
        }
        catch (Exception ex)
        {
            return Result.Error($"Adapter '{concernName}' konnte nicht gestartet werden: {exeFileName}", ex);
        }

        if (process is null)
            return Result.Error($"Process.Start lieferte null für Adapter '{concernName}': {exeFileName}");

        var connect = await PipesClient.ConnectAsync(sessionGuid);
        if (!connect.Succeeded)
        {
            try { process.Kill(); process.Dispose(); } catch { /* best effort */ }
            return Result<AdapterProcess>.Propagate(
                Result.Error($"Verbindung zu Adapter '{concernName}' fehlgeschlagen: {connect.ErrorMessage}"));
        }

        return new AdapterProcess(concernName, sessionGuid, process, connect.Value);
    }
}
