using JOSYN.Backend.GlobalConfig;
using JOSYN.Backend.SessionStarter;
using JOSYN.Backend.SessionStore;



// Demo INI arguments — same content the old JAPServer fake returned.
const string iniArguments =
    """
    Msg=Hello JOSYN
    Count=9
    MaybeCount=
    IsSpecial=True
    Expired=21.09.1988 00:00:00
    OnlyDate=04.11.1966
    MaybeDate=
    EnumValue=Value2
    MyTimeSpan=09:10:59
    Price=1.200,30
    """;

var config = new HardcodedGlobalConfig();
var sessionStore = new SessionStore(config.SessionStoreConnectionString);
var starter = new SessionStarter(sessionStore, config);

try
{

    Console.WriteLine("[DEMO] Starte Session...");
    var start = starter.StartSession("DemoJob", iniArguments);
    if (!start.Succeeded)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FEHLER] Session konnte nicht gestartet werden: {start.ErrorMessage}");
        Console.ResetColor();
        return 1;
    }

    var guid = start.Value;
    Console.WriteLine($"[DEMO] Session gestartet. GUID: {guid}");
    Console.WriteLine("[DEMO] Warte auf JAPServer-Ergebnis...");

    var deadline = DateTime.UtcNow.AddSeconds(30);
    string? lastPollError = null;

    while (DateTime.UtcNow < deadline)
    {
        await Task.Delay(500);

        var get = sessionStore.GetSession(guid);
        if (!get.Succeeded)
        {
            lastPollError = get.ErrorMessage;
            continue;
        }

        lastPollError = null;
        if (!string.IsNullOrEmpty(get.Value.Result))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[DEMO] Ergebnis empfangen:");
            Console.WriteLine(get.Value.Result);
            Console.ResetColor();
            return 0;
        }
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(lastPollError is null
        ? "[DEMO] Timeout: Kein Ergebnis nach 30 Sekunden."
        : $"[DEMO] Timeout. Letzter DB-Fehler: {lastPollError}");
    Console.ResetColor();

    return 1;

}
finally
{
    Console.WriteLine("[PRESS KEY]");
    Console.ReadKey();
}


