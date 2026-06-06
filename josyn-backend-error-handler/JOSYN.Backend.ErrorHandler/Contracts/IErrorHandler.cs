using System.Runtime.CompilerServices;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// Platform-wide error reporting contract for backend components.
/// <para>
/// Implementations are responsible for durable storage (primary) and local-log
/// fallback (secondary). The contract is fire-and-forget: callers are never
/// blocked by or made aware of persistence failures.
/// </para>
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Reports an error from a raw string context.
    /// <para>
    /// <paramref name="callStack"/> and <paramref name="exceptionDetails"/> are required to
    /// prevent silent loss of diagnostic context. Pass <c>null</c> explicitly when genuinely
    /// unavailable — this is a deliberate acknowledgement, not an oversight.
    /// When a <see cref="Result"/> is in scope, prefer the
    /// <see cref="Handle(Result, string?, Guid?, string, string)"/> overload instead.
    /// </para>
    /// </summary>
    /// <param name="message">Human-readable error description.</param>
    /// <param name="callStack">Serialized Result call chain, or <c>null</c> if none exists.</param>
    /// <param name="exceptionDetails">Serialized exception chain, or <c>null</c> if none exists.</param>
    /// <param name="jobName">Job name if error is job-related; <c>null</c> for system errors.</param>
    /// <param name="sessionGuid">Session GUID if established; <c>null</c> otherwise.</param>
    /// <param name="caller">Automatically captured — do not provide explicitly.</param>
    /// <param name="callerFile">Automatically captured — do not provide explicitly.</param>
    void Handle(
        string   message,
        string?  callStack,
        string?  exceptionDetails,
        string?  jobName        = null,
        Guid?    sessionGuid    = null,
        [CallerMemberName] string caller     = "",
        [CallerFilePath]   string callerFile = "");

    /// <summary>
    /// Reports an error from a <see cref="Result"/>, extracting all diagnostic context automatically.
    /// This is the preferred overload when a failed Result is available at the call site.
    /// </summary>
    /// <param name="result">The failed result. Must not be a succeeded result.</param>
    /// <param name="jobName">Job name if error is job-related; <c>null</c> for system errors.</param>
    /// <param name="sessionGuid">Session GUID if established; <c>null</c> otherwise.</param>
    /// <param name="caller">Automatically captured — do not provide explicitly.</param>
    /// <param name="callerFile">Automatically captured — do not provide explicitly.</param>
    void Handle(
        Result   result,
        string?  jobName     = null,
        Guid?    sessionGuid = null,
        [CallerMemberName] string caller     = "",
        [CallerFilePath]   string callerFile = "");
}
