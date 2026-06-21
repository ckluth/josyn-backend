using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

/// <summary>
/// Write-side contract for job argument records.
/// Deliberately separate from the read-focused <see cref="IJobRegistry"/> so that readers do not
/// take a dependency on mutation capabilities.
/// </summary>
public interface IJobArgumentWriter
{
    /// <summary>
    /// Changes the content of an existing argument record.
    /// </summary>
    /// <returns>
    /// <see cref="ArgumentChangeOutcome"/> with the before and after content on success.
    /// A <see cref="Result.Error"/> with a <c>[NotFound]</c> prefix when the job or the argument
    /// record does not exist — it never creates a new record (use a dedicated create command for that).
    /// </returns>
    Result<ArgumentChangeOutcome> ChangeArgument(string jobName, string argumentName, string content);
}
