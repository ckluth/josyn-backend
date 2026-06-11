namespace JOSYN.Backend.SessionStore;

public interface IJobSessionRecord
{
    Guid      UID               { get; init; }
    string    JobTypeName       { get; init; }
    string    Arguments         { get; init; }
    string    Result            { get; init; }
    string    JobVersion        { get; init; }
    string    UserName          { get; init; }
    string    UserDomain        { get; init; }
    string    ClientApplication { get; init; }
    string    ClientMachine     { get; init; }
    string?   TecUser           { get; init; }
    DateTime  Started           { get; init; }
    string    ExecutionStatus   { get; init; }
    string?   Progress          { get; init; }
    DateTime? Finished          { get; init; }
    int       JapServerProcess  { get; init; }
    int       JobHostProcessId  { get; init; }
    int       JapExitCode       { get; init; }
    int       JobExitCode       { get; init; }
    DateTime? LastWriteTime     { get; init; }
    string?   WrittenBy         { get; init; }
}
