using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Jap.JAPServer;

/// <summary>
/// Holds all spawned adapter processes for the current JAPServer session.
/// Keyed by concern name (e.g. <c>"IdentityAdapter"</c>).
/// </summary>
internal sealed class AdapterManager : IDisposable
{
    private readonly Dictionary<string, AdapterProcess> _adapters = new(StringComparer.OrdinalIgnoreCase);

    internal void Add(AdapterProcess adapter) => _adapters[adapter.ConcernName] = adapter;

    /// <summary>
    /// Returns the open <see cref="ClientPipes"/> for the given concern.
    /// Fails if no adapter with that name was spawned.
    /// </summary>
    internal Result<ClientPipes> GetPipes(string concernName) =>
        _adapters.TryGetValue(concernName, out var adapter)
            ? Result<ClientPipes>.Success(adapter.Pipes)
            : Result.Error($"Adapter '{concernName}' wurde nicht gestartet.");

    public void Dispose()
    {
        foreach (var adapter in _adapters.Values)
        {
            try { adapter.Dispose(); } catch { /* best effort — dispose all regardless */ }
        }
        _adapters.Clear();
    }
}
