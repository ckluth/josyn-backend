using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.Contracts;

namespace JOSYN.Backend.CLI;

internal class Program
{
    private static int Main(string[] args) { return RunJob(args); }

    private static int RunJob(string[] args)
    {
        try
        {
            //
            // Parse CLI Arguments
            //
            if (args.Length < 2 || !string.Equals(args[0], "run-job", StringComparison.OrdinalIgnoreCase))
                return PrintMessage("Verwendung: JOSYN.Backend.CLI.exe run-job <jobname> [<argumentdatei>]", 1,
                    MsgType.Error);

            var jobTypeName = args[1];
            var arguments = string.Empty;

            //
            // Load Job Arguments (optional)
            //        
            if (args.Length >= 3)
            {
                var argFile = args[2];
                if (!File.Exists(argFile))
                    return PrintMessage($"Argumentdatei nicht gefunden: '{argFile}'", 1, MsgType.Error);

                arguments = Convert.ToBase64String(File.ReadAllBytes(argFile));
            }

            //
            // Load Bootstrap Configuration
            //
            var loadConfig = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", FileBootstrapConfig.FileName));
            if (!loadConfig.Succeeded)
                return PrintMessage($"Bootstrap-Konfiguration konnte nicht geladen werden: {loadConfig.ErrorMessage}", 1, MsgType.Error);

            var msg =
                $"Starte Job-Session...\n" +
                $"  Job       : {jobTypeName}\n" +
                $"  Argumente : {(string.IsNullOrEmpty(arguments) ? "(keine)" : args[2])}\n";
            PrintMessage(msg);

            //
            // Launch Session
            //
            var config = loadConfig.Value;
            var jobRegistry = new SqlJobRegistry(config.SessionStoreConnectionString);
            var launcher = new SessionLauncher.SessionLauncher(config, jobRegistry);

            var result = launcher.LaunchSession(new SessionLaunchRequest
            {
                JobTypeName = jobTypeName,
                Arguments = arguments,
                CallerUser = Environment.UserName,
                CallerDomain = Environment.UserDomainName,
                CallerApplication = AppDomain.CurrentDomain.FriendlyName,
                CallerMachine = Environment.MachineName
            });

            return !result.Succeeded
                ? PrintMessage($"Session konnte nicht gestartet werden: {result.ErrorMessage}", 1, MsgType.Error)
                : PrintMessage("Session gestartet.", 0, MsgType.Success);

        }
        catch (Exception ex)
        {
            return PrintMessage($"Unerwarteter Fehler beim Ausführen von CLI.exe: {ex.Message}", 1, MsgType.Error);
        }
    }

    private enum MsgType
    {
        Info,
        Success,
        Error
    }

    private static int PrintMessage(string msg, int exitCode = 0, MsgType msgTyp = MsgType.Info)
    {
        Console.ForegroundColor = msgTyp switch
        {
            MsgType.Error => ConsoleColor.Red,
            MsgType.Success => ConsoleColor.Green,
            _ => ConsoleColor.White
        };

        if (msgTyp == MsgType.Error)
            Console.Error.WriteLine(msg);
        else
            Console.WriteLine(msg);

        Console.ResetColor();
        return exitCode;
    }

}



