using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.BootstrapConfig;

/// <summary>
/// File-based implementation of <see cref="IBootstrapConfig"/>.
/// Reads bootstrap values from a flat INI file (<c>josyn.bootstrap.ini</c>).
/// </summary>
/// <remarks>
/// Use <see cref="Load"/> to create an instance. All required keys are validated
/// at load time — if any are missing the Result fails and the backend must not start.
/// </remarks>
public sealed class FileBootstrapConfig : IBootstrapConfig
{
    private static readonly string[] RequiredKeys =
    [
        nameof(IBootstrapConfig.SessionStoreConnectionString),
        nameof(IBootstrapConfig.JapServerExePath),
        nameof(IBootstrapConfig.JobRepositoryRoot),
    ];

    private readonly Dictionary<string, string> _values;

    private FileBootstrapConfig(Dictionary<string, string> values) => _values = values;

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

            var raw    = File.ReadAllText(path);
            var parsed = IniDictionarySerializer.DeserializeSingleSection(raw);
            if (!parsed.Succeeded)
                return Result<FileBootstrapConfig>.Propagate(parsed.ToResult<FileBootstrapConfig>());

            var missing = RequiredKeys.Where(k => !parsed.Value.ContainsKey(k)).ToArray();
            if (missing.Length > 0)
                return Result.Error($"Fehlende Schlüssel in '{path}': {string.Join(", ", missing)}");

            return new FileBootstrapConfig(parsed.Value);
        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc/>
    public string SessionStoreConnectionString => _values[nameof(IBootstrapConfig.SessionStoreConnectionString)];

    /// <inheritdoc/>
    public string JapServerExePath => _values[nameof(IBootstrapConfig.JapServerExePath)];

    /// <inheritdoc/>
    public string JobRepositoryRoot => _values[nameof(IBootstrapConfig.JobRepositoryRoot)];
}
