using JOSYN.Backend.ErrorHandler;
using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.SessionStarter;
using JOSYN.Backend.SessionStore;

// Hardcoded demo values — replace with real inputs once CLI arg parsing is added.
const string DemoJobTypeName = "MyDemoCompany.MyDemoProduct.MyDemoJob";
const string DemoArguments   =
    "Msg=Hallo JOSYN\n" +
    "Count=5\n" +
    "MaybeCount=\n" +
    "IsSpecial=True\n" +
    "OnlyDate=01.07.2025\n" +
    "Expired=01.06.2025 08:00:00\n" +
    "MaybeDateTime=\n" +
    "EnumValue=Value2\n" +
    "MyTimeSpan=00:01:30\n" +
    "Price=19,99\n";

try
{
    HardCodedDemoSessionStart();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
finally
{
    Console.WriteLine("press any key to exit...");
    Console.ReadKey();
}

// -------------------------------------------------------------------------

static void HardCodedDemoSessionStart()
{
    var config       = FileBootstrapConfig.Load("josyn.bootstrap.ini").Value!;
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

