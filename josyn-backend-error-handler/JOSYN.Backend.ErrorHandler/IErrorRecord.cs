namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// A durable error record stored in <c>josyn.ErrorStore</c>.
/// </summary>
/// <remarks>
/// Error kind is inferred from the nullable context fields:
/// <list type="bullet">
///   <item><description>System error: <see cref="JobName"/> and <see cref="SessionGuid"/> are both <c>null</c>.</description></item>
///   <item><description>Job error without session: <see cref="JobName"/> is set, <see cref="SessionGuid"/> is <c>null</c>.</description></item>
///   <item><description>Job error with session: both <see cref="JobName"/> and <see cref="SessionGuid"/> are set.</description></item>
/// </list>
/// </remarks>
public interface IErrorRecord
{
    /// <summary>Unique identifier generated on storage.</summary>
    Guid            UID              { get; init; }

    /// <summary>When the error occurred.</summary>
    DateTimeOffset  OccurredAt       { get; init; }

    /// <summary>Method name of the backend component that observed and reported the error.</summary>
    string          Causer           { get; init; }

    /// <summary>Human-readable error description.</summary>
    string          Message          { get; init; }

    /// <summary>Serialized Result call chain or stack trace, if available.</summary>
    string?         CallStack        { get; init; }

    /// <summary>Serialized exception, if available.</summary>
    string?         ExceptionDetails { get; init; }

    /// <summary>Name of the job involved, or <c>null</c> for system errors.</summary>
    string?         JobName          { get; init; }

    /// <summary>Session GUID if a session was established, or <c>null</c>.</summary>
    Guid?           SessionGuid      { get; init; }
}
