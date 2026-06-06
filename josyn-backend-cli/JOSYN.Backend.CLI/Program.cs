using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.SessionStarter;
using JOSYN.Backend.SessionStore;

// Hardcoded demo values — replace with real inputs once CLI arg parsing is added.
const string DemoJobTypeName = "Contoso.DemoProduct.DemoJob";
const string DemoArguments   =
    "Message=Hallo JOSYN\n" +
    "RepeatCount=3\n" +
    "ScheduledFor=06.06.2025\n" +
    "IsHighPriority=False\n" +
    "Budget=1499,99\n";

try
{
    HardCodedDemoSessionStart();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadKey();
}

// -------------------------------------------------------------------------

static void HardCodedDemoSessionStart()
{
    var loadConfig   = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName));
    if (!loadConfig.Succeeded)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Bootstrap-Konfiguration konnte nicht geladen werden: {loadConfig.ErrorMessage}");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }
    var config       = loadConfig.Value;
    var errorHandler = new SqlErrorHandler(config.SessionStoreConnectionString);
    var sessionStore = new SessionStore(config.SessionStoreConnectionString);
    var jobRegistry  = new SqlJobRegistry(config.SessionStoreConnectionString);
    var starter      = new SessionStarter(sessionStore, config, jobRegistry);

    Console.WriteLine("Starting demo session...");
    Console.WriteLine($"  JobTypeName : {DemoJobTypeName}");
    Console.WriteLine($"  Arguments   : {(string.IsNullOrEmpty(DemoArguments) ? "(none)" : DemoArguments)}");
    Console.WriteLine();

    var result = starter.StartSession(DemoJobTypeName, DemoArguments);

    if (!result.Succeeded)
    {
        var msg = $"Session konnte nicht gestartet werden: {result.ErrorMessage}";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
        errorHandler.Handle(result.ToResult());
        return;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Session gestartet. GUID: {result.Value}");
    Console.ResetColor();
}

