using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.SessionStarter;
using JOSYN.Backend.SessionStore;

return RunJob(args);

// -------------------------------------------------------------------------

static int RunJob(string[] args)
{
    if (args.Length < 2 || !string.Equals(args[0], "run-job", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("Verwendung: JOSYN.Backend.CLI.exe run-job <jobname> [<argumentdatei>]");
        return 1;
    }

    var jobTypeName = args[1];
    var arguments   = string.Empty;

    if (args.Length >= 3)
    {
        var argFile = args[2];
        if (!File.Exists(argFile))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Argumentdatei nicht gefunden: '{argFile}'");
            Console.ResetColor();
            return 1;
        }
        arguments = File.ReadAllText(argFile);
    }

    var loadConfig = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName));
    if (!loadConfig.Succeeded)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Bootstrap-Konfiguration konnte nicht geladen werden: {loadConfig.ErrorMessage}");
        Console.ResetColor();
        return 1;
    }

    var config       = loadConfig.Value;
    var errorHandler = new SqlErrorHandler(config.SessionStoreConnectionString);
    var sessionStore = new SessionStore(config.SessionStoreConnectionString);
    var jobRegistry  = new SqlJobRegistry(config.SessionStoreConnectionString);
    var starter      = new SessionStarter(sessionStore, config, jobRegistry);

    Console.WriteLine($"Starte Job-Session...");
    Console.WriteLine($"  Job       : {jobTypeName}");
    Console.WriteLine($"  Argumente : {(string.IsNullOrEmpty(arguments) ? "(keine)" : args[2])}");
    Console.WriteLine();

    var result = starter.StartSession(jobTypeName, arguments);

    if (!result.Succeeded)
    {
        var msg = $"Session konnte nicht gestartet werden: {result.ErrorMessage}";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(msg);
        Console.ResetColor();
        errorHandler.Handle(result.ToResult());
        return 1;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Session gestartet. GUID: {result.Value}");
    Console.ResetColor();
    return 0;
}
