using JOSYN.Foundation.JIP;
using System.Diagnostics;

namespace JOSYN.Jap.JAPServer;

/// <summary>
/// Represents a single spawned adapter process and its open JIP pipe connection.
/// </summary>
internal sealed class AdapterProcess(string concernName, Guid sessionGuid, Process process, ClientPipes pipes) : IDisposable
{
    internal string      ConcernName { get; } = concernName;
    internal Guid        SessionGuid { get; } = sessionGuid;
    internal Process     Process     { get; } = process;
    internal ClientPipes Pipes       { get; } = pipes;

    public void Dispose()
    {
        try { PipesClient.DisconnectAsync(Pipes, sendShutdownRequest: true).GetAwaiter().GetResult(); } catch { /* best effort */ }
        try { if (!Process.HasExited) Process.Kill(); } catch { /* best effort */ }
        Process.Dispose();
    }
}
