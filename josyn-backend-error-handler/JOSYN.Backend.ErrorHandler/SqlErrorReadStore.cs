using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// Reads error records from <c>josyn.ErrorStore</c> via EF Core.
/// </summary>
public sealed class SqlErrorReadStore(string connectionString) : IErrorReadStore
{
    /// <inheritdoc/>
    public Result<IErrorRecord> GetByUid(Guid uid)
    {
        try
        {
            using var ctx = new ErrorStoreDbContext(connectionString);
            var entity = ctx.ErrorStore
                .AsNoTracking()
                .FirstOrDefault(e => e.UID == uid);

            if (entity is null)
                return Result.Error($"Kein Fehlerdatensatz gefunden für UID '{uid}'.");

            return Result<IErrorRecord>.Success(new ErrorRecord
            {
                UID              = entity.UID,
                OccurredAt       = entity.OccurredAt,
                Causer           = entity.Causer,
                Message          = entity.Message,
                CallStack        = entity.CallStack,
                ExceptionDetails = entity.ExceptionDetails,
                JobName          = entity.JobName,
                SessionGuid      = entity.SessionGuid
            });
        }
        catch (Exception ex) { return ex; }
    }
}
