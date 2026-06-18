// TimeScheduler — Ticker target (ADR-024).
// Finds the next due job, loads its default arguments, and launches a session.

using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.Contracts;
using JOSYN.Foundation.ResultPattern;
using Launcher = JOSYN.Backend.SessionLauncher;

namespace TimeScheduler;

internal class Program
{
    private static int Main(string[] args)
    {
        Log("TimeScheduler invoked.");

        try
        {
            return RunScheduledSessions();
        }
        catch (Exception ex)
        {
            return Fail($"Unexpected failure: {ex.Message}");
        }
    }

    private static int RunScheduledSessions()
    {
        // Convention: orchestrators live at depth 2 under the platform root (Orchestrators\<Name>\).
        // bootstrap.ini lives at the platform root — two levels up.
        var loadConfig = FileBootstrapConfig.Load(
            Path.Combine(AppContext.BaseDirectory, "..", "..", FileBootstrapConfig.FileName));

        if (!loadConfig.Succeeded)
            return Fail($"Could not load bootstrap configuration: {loadConfig.ErrorMessage}");

        var config = loadConfig.Value;

        var jobTypeName = FindNextDueJobTypeName();
        if (jobTypeName is null)
        {
            Log("No job due. Exiting.");
            return 0;
        }

        return LaunchSession(jobTypeName, config);
    }

    private static int LaunchSession(string jobTypeName, IBootstrapConfig config)
    {
        var loadArguments = LoadJobArguments(config.BackendRoot, jobTypeName);
        if (!loadArguments.Succeeded)
            return Fail($"Could not load arguments for '{jobTypeName}': {loadArguments.ErrorMessage}");

        Log($"Launching session for '{jobTypeName}'...");

        var result = Launcher.SessionLauncher.LaunchSession(new SessionLaunchRequest
        {
            JobTypeName       = jobTypeName,
            Arguments         = loadArguments.Value,
            CallerUser        = Environment.UserName,
            CallerDomain      = Environment.UserDomainName,
            CallerApplication = AppDomain.CurrentDomain.FriendlyName,
            CallerMachine     = Environment.MachineName,
            Interactive       = true    // TODO: revert to false before production deployment — dev/integration visibility only
        },
            config.BackendRoot, new SqlJobRegistry(config.SessionStoreConnectionString));

        if (!result.Succeeded)
            return Fail($"Session launch failed for '{jobTypeName}': {result.ErrorMessage}");

        Log($"Session launched successfully for '{jobTypeName}'.");
        return 0;
    }

    // ── scheduling stub ───────────────────────────────────────────────────────
    // TODO: replace with real time-based logic that queries the schedule store
    //       and returns the job type name of the next due execution, or null
    //       when nothing is currently due.
    private static string? FindNextDueJobTypeName() => "Contoso.DemoProduct.DemoJob";

    // ── arguments ─────────────────────────────────────────────────────────────
    // Loads the default arguments file from the job repository convention:
    //   <BackendRoot>\JobRepository\<JobTypeName>\local-arguments\arguments-default.ini
    // Returns the file content base64-encoded, ready for SessionLaunchRequest.Arguments.
    private static Result<string> LoadJobArguments(string backendRoot, string jobTypeName)
    {
        var path = Path.Combine(
            backendRoot, "JobRepository", jobTypeName, "local-arguments", "arguments-default.ini");

        if (!File.Exists(path))
            return Result.Error($"Arguments file not found: '{path}'");

        return Convert.ToBase64String(File.ReadAllBytes(path));
    }

    // ── logging ───────────────────────────────────────────────────────────────
    private static int Fail(string message)
    {
        Log($"[ERROR] {message}");
        return 1;
    }

    private static void Log(string message)
    {
        var entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
        Console.WriteLine(entry);
        WriteToLogFile(entry);
    }

    private static void WriteToLogFile(string entry)
    {
        try
        {
            var logDir  = Path.Combine(AppContext.BaseDirectory, "logs");
            var logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}.log");

            Directory.CreateDirectory(logDir);
            File.AppendAllText(logFile, entry + Environment.NewLine);
        }
        catch
        {
            // Log-write failures are non-fatal — the session was already launched (or failed).
            // A missing log entry is acceptable; aborting the scheduler over a log I/O error is not.
        }
    }
}
