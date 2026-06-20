// JOSYN.Backend.TimeScheduler — Ticker target (ADR-024).
// Loads all job schedules from the DB, evaluates which entries are due,
// resolves their argument records, and launches one session per due entry.

using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.JobScheduleStore;
using JOSYN.Backend.Contracts;
using JOSYN.Commons.Schedule;
using JOSYN.Foundation.ResultPattern;
using Launcher = JOSYN.Backend.SessionLauncher;

namespace JOSYN.Backend.TimeScheduler;

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
        var configPath = Path.Combine(AppContext.BaseDirectory, "..", "..", FileBootstrapConfig.FileName);
        var loadConfig = FileBootstrapConfig.Load(configPath);

        if (!loadConfig.Succeeded)
            return Fail($"Could not load bootstrap configuration: {loadConfig.ErrorMessage}");

        var config       = loadConfig.Value;
        var errorHandler = new SqlErrorHandler(config.SessionStoreConnectionString);

        Log($"Config loaded from: {Path.GetFullPath(configPath)}");

        var loadSchedules = new SqlJobScheduleStore(config.SessionStoreConnectionString).GetAll();
        if (!loadSchedules.Succeeded)
        {
            errorHandler.Handle(
                loadSchedules.ErrorMessage,
                loadSchedules.CallStackAsString,
                loadSchedules.Exception?.ToString());
            return Fail($"Could not load job schedules: {loadSchedules.ErrorMessage}");
        }

        var schedules = loadSchedules.Value.ToList();
        Log($"Loaded {schedules.Count} schedule(s).");

        if (schedules.Count == 0)
        {
            Log("No schedules found — nothing to do.");
            return 0;
        }

        var registry = new SqlJobRegistry(config.SessionStoreConnectionString);
        var now      = DateTime.Now;
        var launched = 0;
        var errors   = 0;

        foreach (var schedule in schedules)
        {
            if (IsSuspended(schedule, now))
            {
                Log($"Skipping '{schedule.JobName}' — suspended" +
                    (schedule.SuspendedUntil.HasValue ? $" until {schedule.SuspendedUntil}" : "."));
                continue;
            }

            foreach (var entry in schedule.Entries)
            {
                if (!IsDue(entry, now))
                    continue;

                if (LaunchEntry(schedule.JobName, entry, config, registry, errorHandler).Succeeded)
                    launched++;
                else
                    errors++;
            }
        }

        Log($"TimeScheduler completed. {launched} session(s) launched, {errors} error(s).");
        return errors == 0 ? 0 : 1;
    }

    private static Result LaunchEntry(
        string jobName, IJobScheduleEntryRecord entry,
        IBootstrapConfig config, IJobRegistry registry,
        IErrorHandler errorHandler)
    {
        var loadArgument = registry.GetArgument(jobName, entry.ArgumentRecordName);
        if (!loadArgument.Succeeded)
        {
            errorHandler.Handle(
                loadArgument.ErrorMessage,
                loadArgument.CallStackAsString,
                loadArgument.Exception?.ToString(),
                jobName);
            Fail($"Could not load argument record '{entry.ArgumentRecordName}' for '{jobName}': {loadArgument.ErrorMessage}");
            return Result.Error(loadArgument.ErrorMessage);
        }

        Log($"Launching '{jobName}' / '{entry.ArgumentRecordName}'...");

        var result = Launcher.SessionLauncher.LaunchSession(new SessionLaunchRequest
        {
            JobTypeName       = jobName,
            Arguments         = Convert.ToBase64String(
                                    System.Text.Encoding.UTF8.GetBytes(loadArgument.Value.Content)),
            CallerUser        = Environment.UserName,
            CallerDomain      = Environment.UserDomainName,
            CallerApplication = AppDomain.CurrentDomain.FriendlyName,
            CallerMachine     = Environment.MachineName,
            Interactive       = true    // TODO: revert to false before production deployment
        },
            config.BackendRoot, new SqlJobRegistry(config.SessionStoreConnectionString));

        if (!result.Succeeded)
        {
            errorHandler.Handle(result, jobName);
            Fail($"Session launch failed for '{jobName}' / '{entry.ArgumentRecordName}': {result.ErrorMessage}");
            return result;
        }

        Log($"Session launched for '{jobName}' / '{entry.ArgumentRecordName}'.");
        return Result.Success;
    }

    // ── suspension check ──────────────────────────────────────────────────────
    private static bool IsSuspended(IJobScheduleRecord schedule, DateTime now)
    {
        if (!schedule.Suspended)
            return false;

        // Auto-lift: treat as not suspended once SuspendedUntil has passed.
        if (schedule.SuspendedUntil.HasValue && DateOnly.FromDateTime(now) > schedule.SuspendedUntil.Value)
            return false;

        return true;
    }

    // ── schedule evaluation ───────────────────────────────────────────────────

    private static bool IsDue(IJobScheduleEntryRecord entry, DateTime now)
    {
        var parseResult = ScheduleParser.Parse(entry.ScheduleDefinition);
        if (!parseResult.Succeeded)
        {
            Log($"[WARN] Could not parse schedule for entry '{entry.ArgumentRecordName}': " +
                parseResult.ErrorMessage);
            return false;
        }

        return ScheduleEvaluator.IsDue(parseResult.Value, now);
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

    private static void WriteToLogFile(string logEntry)
    {
        try
        {
            var logDir  = Path.Combine(AppContext.BaseDirectory, "logs");
            var logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}.log");

            Directory.CreateDirectory(logDir);
            File.AppendAllText(logFile, logEntry + Environment.NewLine);
        }
        catch
        {
            // Log-write failures are non-fatal — the session was already launched (or failed).
        }
    }
}
