namespace JOSYN.Backend.Contracts;

/// <summary>
/// The closed set of permitted values for <see cref="IJobSessionRecord.ExecutionStatus"/>.
/// See ADR-016 for the full state machine and value semantics.
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// JAPServer is active; job.exe is launching, connecting pipes,
    /// and the accept/reject negotiation (ADR-008) is in progress.
    /// </summary>
    Preparing,

    /// <summary>Both processes are active; job is executing.</summary>
    Running,

    /// <summary>A cancellation signal has been issued; the job has not yet honoured it.</summary>
    RunningCancellationRequested,

    /// <summary>Job completed and called PutRawResult with a success result.</summary>
    FinishedSuccessfully,

    /// <summary>
    /// Job ran to completion and called PutRawResult with a domain error result —
    /// technically clean exit, but the job itself determined something went wrong in its subject area.
    /// </summary>
    FinishedWithErrors,

    /// <summary>
    /// An unhandled exception bubbled out of the job entry point; job-host called PutError.
    /// The job never reached a deliberate outcome.
    /// </summary>
    FinishedFaulted,

    /// <summary>Job honoured the cancellation request and terminated.</summary>
    FinishedByCancellation,

    /// <summary>
    /// Session was accepted by the scheduler but rejected during pre-execution negotiation.
    /// </summary>
    FinishedRejected,

    /// <summary>
    /// Processes died without reporting any outcome; detected and written by an external watchdog.
    /// </summary>
    FinishedAbandoned
}
