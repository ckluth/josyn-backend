using JOSYN.Commons.Log;
using JOSYN.Foundation.ResultPattern;
using System.Runtime.CompilerServices;

namespace JOSYN.Backend.ErrorHandler;

/// <summary>
/// Production error handler: persists error records to <c>josyn.ErrorStore</c> via EF Core.
/// Falls back to <see cref="LocalLog"/> when SQL storage is unavailable.
/// Never throws — all failures are swallowed after the fallback write.
/// </summary>
public sealed class SqlErrorHandler(string connectionString) : IErrorHandler
{
    /// <inheritdoc/>
    public void Handle(
        string  message,
        string? callStack,
        string? exceptionDetails,
        string? jobName          = null,
        Guid?   sessionGuid      = null,
        [CallerMemberName] string caller = "")
    {
        var record = new ErrorStoreEntity
        {
            UID              = Guid.NewGuid(),
            OccurredAt       = DateTimeOffset.Now,
            Causer           = caller,
            Message          = message,
            CallStack        = callStack,
            ExceptionDetails = exceptionDetails,
            JobName          = jobName,
            SessionGuid      = sessionGuid
        };

        try
        {
            using var ctx = new ErrorStoreDbContext(connectionString);
            ctx.ErrorStore.Add(record);
            ctx.SaveChanges();
        }
        catch
        {
            // Primary storage failed — fall back to local log.
            LocalLog.WriteError(
                caller,
                message,
                callStack,
                exceptionDetails);
        }
    }

    /// <inheritdoc/>
    public void Handle(
        Result  result,
        string? jobName     = null,
        Guid?   sessionGuid = null,
        [CallerMemberName] string caller = "")
        => Handle(
            result.ErrorMessage ?? "(kein Fehlertext)",
            result.CallStackAsString,
            result.Exception?.ToString(),
            jobName,
            sessionGuid,
            caller);
}
