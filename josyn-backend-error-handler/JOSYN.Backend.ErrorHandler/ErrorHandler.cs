using JOSYN.Backend.GlobalConfig;

namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// First-version error handler: writes timestamped entries to <c>Console.Error</c> and
/// appends them to a log file in the system temp folder.
/// Known limitation: log path is hard-coded. Replace when a log-path property is added
/// to <see cref="IGlobalConfig"/>.
/// </summary>
public sealed class FileSystemErrorHandler(IGlobalConfig _) : IErrorHandler
{
    private static readonly string LogPath =
        Path.Combine(Path.GetTempPath(), "josyn-error.log");

    /// <inheritdoc/>
    public void Handle(string message) => Write(message);

    /// <inheritdoc/>
    public void Handle(string message, Exception exception) =>
        Write($"{message}: {exception}");

    private static void Write(string text)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
        Console.Error.WriteLine(line);
        try { File.AppendAllText(LogPath, line + Environment.NewLine); }
        catch { /* swallow: if the log itself fails, there is nowhere to report it */ }
    }
}
