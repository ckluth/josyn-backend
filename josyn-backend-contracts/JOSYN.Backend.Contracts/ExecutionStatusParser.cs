using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.Contracts;

/// <summary>
/// Converts between the <see cref="ExecutionStatus"/> enum and its canonical DB string
/// representation (see ADR-016).
/// </summary>
public static class ExecutionStatusParser
{
    /// <summary>
    /// Parses a raw DB string into an <see cref="ExecutionStatus"/> value.
    /// Returns a failed <see cref="Result{T}"/> for any unrecognised value rather than throwing,
    /// in line with the platform Result pattern.
    /// </summary>
    public static Result<ExecutionStatus> Parse(string value) =>
        value switch
        {
            "preparing"                      => ExecutionStatus.Preparing,
            "running"                        => ExecutionStatus.Running,
            "running-cancellation-requested" => ExecutionStatus.RunningCancellationRequested,
            "finished-successfully"          => ExecutionStatus.FinishedSuccessfully,
            "finished-with-errors"           => ExecutionStatus.FinishedWithErrors,
            "finished-faulted"               => ExecutionStatus.FinishedFaulted,
            "finished-by-cancellation"       => ExecutionStatus.FinishedByCancellation,
            "finished-rejected"              => ExecutionStatus.FinishedRejected,
            "finished-abandoned"             => ExecutionStatus.FinishedAbandoned,
            _                                => Result.Error($"Unbekannter ExecutionStatus-Wert in der Datenbank: '{value}'")
        };

    /// <summary>
    /// Serializes an <see cref="ExecutionStatus"/> value to its canonical DB string.
    /// This direction is always safe — all enum members are covered.
    /// </summary>
    public static string Serialize(ExecutionStatus status) =>
        status switch
        {
            ExecutionStatus.Preparing                    => "preparing",
            ExecutionStatus.Running                      => "running",
            ExecutionStatus.RunningCancellationRequested => "running-cancellation-requested",
            ExecutionStatus.FinishedSuccessfully         => "finished-successfully",
            ExecutionStatus.FinishedWithErrors           => "finished-with-errors",
            ExecutionStatus.FinishedFaulted              => "finished-faulted",
            ExecutionStatus.FinishedByCancellation       => "finished-by-cancellation",
            ExecutionStatus.FinishedRejected             => "finished-rejected",
            ExecutionStatus.FinishedAbandoned            => "finished-abandoned",
            _                                            => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
