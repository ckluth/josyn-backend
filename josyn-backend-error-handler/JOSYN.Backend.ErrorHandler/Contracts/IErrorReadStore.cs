using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// Read-only view of <c>josyn.ErrorStore</c>.
/// </summary>
/// <remarks>
/// Intentionally separate from the write-only <see cref="IErrorHandler"/>: the write contract is
/// fire-and-forget and carries <c>[CallerMemberName]</c> capture; the read contract is a normal
/// query that returns a <see cref="Result{T}"/>. Splitting them mirrors the
/// <c>IJobRegistry</c> / <c>IJobArgumentWriter</c> split in <c>JOSYN.Backend.JobRegistry</c>.
/// </remarks>
public interface IErrorReadStore
{
    /// <summary>
    /// Returns the error record identified by <paramref name="uid"/>, or a
    /// <c>[NotFound]</c>-category failure when no record exists for that UID.
    /// </summary>
    Result<IErrorRecord> GetByUid(Guid uid);
}
