using JOSYN.Backend.ErrorHandler;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <see cref="GetErrorDetail"/>: the full detail of one stored error by its UID.
/// </summary>
internal sealed class GetErrorDetailHandler(string connectionString)
{
    internal Result<ErrorDetail> Handle(GetErrorDetail query)
    {
        IErrorReadStore store = new SqlErrorReadStore(connectionString);
        var storeResult = store.GetByUid(query.ErrorUid);
        if (!storeResult.Succeeded)
            return JrpError.NotFound(
                $"No error found for UID '{query.ErrorUid}' on this installation.");

        return Result<ErrorDetail>.Success(MapError(storeResult.Value, query));

        // ── helpers ──────────────────────────────────────────────────────────
        static ErrorDetail MapError(IErrorRecord r, GetErrorDetail q) => new()
        {
            Uid              = r.UID,
            OccurredAt       = r.OccurredAt,
            Causer           = r.Causer,
            Message          = r.Message,
            CallStack        = r.CallStack,
            ExceptionDetails = r.ExceptionDetails,
            JobName          = r.JobName,
            SessionGuid      = r.SessionGuid,
            Environment      = q.Target.Environment,
            Machine          = q.Target.Machine
        };
    }
}
