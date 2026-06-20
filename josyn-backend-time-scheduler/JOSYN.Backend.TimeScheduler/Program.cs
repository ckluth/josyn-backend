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

        var config         = loadConfig.Value;
        var errorHandler   = new SqlErrorHandler(config.SessionStoreConnectionString);
        var firedSlotStore = new SqlFiredSlotStore(config.SessionStoreConnectionString);
        var registry       = new SqlJobRegistry(config.SessionStoreConnectionString);
        var now            = TruncateToMinute(DateTime.Now);

        Log($"Config loaded from: {Path.GetFullPath(configPath)}");

        PruneFiredSlots(firedSlotStore, now);

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
                var outcome = ProcessEntry(schedule.JobName, entry, now, firedSlotStore, config, registry, errorHandler);
                if      (outcome ==  1) launched++;
                else if (outcome == -1) errors++;
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

    private const int DefaultToleranceMinutes = 1;

    // Returns 1 = launched, 0 = skipped (not due or already fired by a prior tick), -1 = error.
    private static int ProcessEntry(
        string jobName, IJobScheduleEntryRecord entry, DateTime now,
        IFiredSlotStore firedSlotStore, IBootstrapConfig config,
        IJobRegistry registry, IErrorHandler errorHandler)
    {
        var slot = FindLatestSlot(entry, now);
        if (slot is null)
            return 0;

        var insertResult = firedSlotStore.TryInsert(jobName, entry.ArgumentRecordName, slot.Value, now);
        if (!insertResult.Succeeded)
        {
            // DB error — do NOT launch: firing without the log record would break at-most-once.
            errorHandler.Handle(
                insertResult.ErrorMessage,
                insertResult.CallStackAsString,
                insertResult.Exception?.ToString(),
                jobName);
            Fail($"Fired-slot insert failed for '{jobName}' / '{entry.ArgumentRecordName}': {insertResult.ErrorMessage}");
            return -1;
        }

        // Another tick already handled this slot within the tolerance window.
        if (!insertResult.Value)
            return 0;

        return LaunchEntry(jobName, entry, config, registry, errorHandler).Succeeded ? 1 : -1;
    }

    // Steps backward from now through [now − T, now] to find the latest canonical slot S.
    // Returns null when no scheduled fire time falls within the tolerance window.
    private static DateTime? FindLatestSlot(IJobScheduleEntryRecord entry, DateTime now)
    {
        var parseResult = ScheduleParser.Parse(entry.ScheduleDefinition);
        if (!parseResult.Succeeded)
        {
            Log($"[WARN] Could not parse schedule for entry '{entry.ArgumentRecordName}': " +
                parseResult.ErrorMessage);
            return null;
        }

        var def = parseResult.Value;
        var t   = entry.ToleranceMinutes ?? DefaultToleranceMinutes;

        for (var offset = 0; offset <= t; offset++)
        {
            var candidate = now.AddMinutes(-offset);
            if (ScheduleEvaluator.IsDue(def, candidate))
                return candidate;
        }
        return null;
    }

    // Prune fired-slot log: 1-day fixed ceiling + 10-minute cleanup buffer (ADR-029 §3 + OQ1).
    private static void PruneFiredSlots(IFiredSlotStore store, DateTime now)
    {
        var cutoff = now.AddMinutes(-(1440 + 10));
        var result = store.Prune(cutoff);
        if (!result.Succeeded)
            Log($"[WARN] Fired-slot prune failed (non-fatal): {result.ErrorMessage}");
    }

    private static DateTime TruncateToMinute(DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

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
