namespace JOSYN.Backend.Contracts;

/// <summary>
/// Read contract for a stored job session record (see ADR-007).
/// </summary>
public interface IJobSessionRecord
{
    /// <summary>Unique identifier of this job session.</summary>
    Guid            UID               { get; init; }
    /// <summary>Fully qualified name of the job type that ran in this session.</summary>
    string          JobTypeName       { get; init; }
    /// <summary>Serialized job arguments passed to the job at launch.</summary>
    string          Arguments         { get; init; }
    /// <summary>Serialized result returned by the job.</summary>
    string          Result            { get; init; }
    /// <summary>Version of the job assembly that ran.</summary>
    string          JobVersion        { get; init; }
    /// <summary>Name of the user who initiated the session.</summary>
    string          UserName          { get; init; }
    /// <summary>Domain of the user who initiated the session.</summary>
    string          UserDomain        { get; init; }
    /// <summary>Name of the client application that requested the session.</summary>
    string          ClientApplication { get; init; }
    /// <summary>Name of the machine from which the session was requested.</summary>
    string          ClientMachine     { get; init; }
    /// <summary>Optional technical user override; <see langword="null"/> if the initiating user is used directly.</summary>
    string?         TecUser           { get; init; }
    /// <summary>Timestamp when the session was started.</summary>
    DateTime        Started           { get; init; }
    /// <summary>Current execution status of the session.</summary>
    ExecutionStatus ExecutionStatus   { get; init; }
    /// <summary>Optional progress message reported by the job; <see langword="null"/> if none.</summary>
    string?         Progress          { get; init; }
    /// <summary>Timestamp when the session finished; <see langword="null"/> if the session is still running.</summary>
    DateTime?       Finished          { get; init; }
    /// <summary>Process ID of the JAP server process that hosted the session.</summary>
    int             JapServerProcessId { get; init; }
    /// <summary>Process ID of the job host process.</summary>
    int             JobHostProcessId  { get; init; }
    /// <summary>Timestamp of the last write to this record; <see langword="null"/> if the record has never been persisted.</summary>
    DateTime?       LastWriteTime     { get; init; }
    /// <summary>Identifier of the host component that last wrote this record; <see langword="null"/> if the record has never been persisted.</summary>
    string?         Host              { get; init; }
}
