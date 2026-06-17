using JOSYN.Backend.BootstrapConfig;
using JOSYN.Backend.JobRegistry;
using JOSYN.Backend.Contracts;
using Launcher = JOSYN.Backend.SessionLauncher;

namespace JOSYN.Backend.CLI;

internal class Program
{
    private static int Main(string[] args) { return RunJob(args); }
    
    private const string runJobArgument = "run-job";
    private static int RunJob(string[] args)
    {
        try
        {
            //
            // Parse CLI Arguments
            //
            if (args.Length < 2 || !string.Equals(args[0], runJobArgument, StringComparison.OrdinalIgnoreCase))
                return PrintMessage($"Verwendung: JOSYN.Backend.CLI.exe {runJobArgument} <jobname> [<argumentdatei>]", 1,
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
            // Convention: orchestrators live at depth 2 under the platform root (Orchestrators\<Name>\).
            // bootstrap.ini lives at the platform root — two levels up.
            var loadBootStrapConfig = FileBootstrapConfig.Load(Path.Combine(AppContext.BaseDirectory, "..", "..", FileBootstrapConfig.FileName));
            if (!loadBootStrapConfig.Succeeded)
                return PrintMessage($"Bootstrap-Konfiguration konnte nicht geladen werden: {loadBootStrapConfig.ErrorMessage}", 1, MsgType.Error);

            var msg =
                $"Starte Job-Session...\n" +
                $"  Job       : {jobTypeName}\n" +
                $"  Argumente : {(string.IsNullOrEmpty(arguments) ? "(keine)" : args[2])}\n";
            PrintMessage(msg);

            //
            // Launch Session
            //
            var bootstrapConfig = loadBootStrapConfig.Value;
            var jobRegistry = new SqlJobRegistry(bootstrapConfig.SessionStoreConnectionString);
            
            var result = Launcher.SessionLauncher.LaunchSession(new SessionLaunchRequest
            {
                JobTypeName = jobTypeName,
                Arguments = arguments,
                CallerUser = Environment.UserName,
                CallerDomain = Environment.UserDomainName,
                CallerApplication = AppDomain.CurrentDomain.FriendlyName,
                CallerMachine = Environment.MachineName,
                Interactive = true   // CLI-launched sessions are dev/debug/maintenance — keep console output visible
            }, 
                bootstrapConfig.BackendRoot, jobRegistry);

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



