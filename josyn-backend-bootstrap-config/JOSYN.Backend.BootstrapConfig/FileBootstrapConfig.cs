using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.BootstrapConfig;

/// <summary>
/// File-based implementation of <see cref="IBootstrapConfig"/>.
/// Reads bootstrap values from an INI file (<c>josyn.bootstrap.ini</c>).
/// The root (sectionless) keys hold core settings; the optional <c>[Adapters]</c>
/// section maps adapter concern names to EXE filenames.
/// </summary>
/// <remarks>
/// Use <see cref="Load"/> to create an instance. All required keys are validated
/// at load time — if any are missing the Result fails and the backend must not start.
/// </remarks>
public sealed class FileBootstrapConfig : IBootstrapConfig
{
    /// <summary>The conventional filename of the bootstrap configuration file.</summary>
    public const string FileName = "josyn.bootstrap.ini";
    private static readonly string[] RequiredKeys =
    [
        nameof(IBootstrapConfig.SessionStoreConnectionString),
    ];

    private readonly Dictionary<string, string> _root;
    private readonly Dictionary<string, string> _adapters;

    private FileBootstrapConfig(string backendRoot, Dictionary<string, string> root, Dictionary<string, string> adapters)
    {
        BackendRoot = backendRoot;
        _root       = root;
        _adapters   = adapters;
    }

    /// <summary>
    /// Loads and validates bootstrap configuration from the specified INI file path.
    /// </summary>
    /// <param name="path">Absolute path to the <c>josyn.bootstrap.ini</c> file.</param>
    public static Result<FileBootstrapConfig> Load(string path)
    {
        try
        {
            if (!File.Exists(path))
                return Result.Error($"Bootstrap-Konfigurationsdatei nicht gefunden: '{path}'");

            var backendRoot = Path.GetDirectoryName(Path.GetFullPath(path));
            if (backendRoot is null)
                return Result.Error($"BackendRoot konnte nicht aus Pfad abgeleitet werden: '{path}'");

            var raw    = File.ReadAllText(path);
            var parsed = IniDictionarySerializer.Deserialize(raw);
            if (!parsed.Succeeded)
                return Result<FileBootstrapConfig>.Propagate(parsed.ToResult<FileBootstrapConfig>());

            var root     = parsed.Value.TryGetValue(string.Empty, out var r) ? r : [];
            var adapters = parsed.Value.TryGetValue("Adapters",   out var a) ? a : [];

            var missing = RequiredKeys.Where(k => !root.ContainsKey(k)).ToArray();
            if (missing.Length > 0)
                return Result.Error($"Fehlende Schlüssel in '{path}': {string.Join(", ", missing)}");

            return new FileBootstrapConfig(backendRoot, root, adapters);
        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc/>
    public string BackendRoot { get; }

    /// <inheritdoc/>
    public string SessionStoreConnectionString => _root[nameof(IBootstrapConfig.SessionStoreConnectionString)];

    /// <inheritdoc/>
    public string? RuntimeEnvironment =>
        _root.TryGetValue(nameof(IBootstrapConfig.RuntimeEnvironment), out var v) ? v : null;

    /// <inheritdoc/>
    public string? GatewayListenUrl =>
        _root.TryGetValue(nameof(IBootstrapConfig.GatewayListenUrl), out var v) ? v : null;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> Adapters => _adapters;
}
