using JOSYN.Backend.Contracts;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Launch;
using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;
using BackendStatus = JOSYN.Backend.Contracts.ExecutionStatus;
using JrpStatus     = JOSYN.Jrp.Surface.SessionStatus;
using SessionStoreImpl = JOSYN.Backend.SessionStore.SessionStore;
using ISessionStore = JOSYN.Backend.SessionStore.ISessionStore;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <see cref="GetRecentSessions"/>: the most recent sessions, newest first.
/// </summary>
internal sealed class GetRecentSessionsHandler(string connectionString)
{
    internal Result<IReadOnlyList<SessionSummary>> Handle(GetRecentSessions query)
    {
        if (query.MaxCount <= 0)
            return JrpError.Invalid(
                $"{nameof(query.MaxCount)} must be positive, was {query.MaxCount}.");

        ISessionStore store = new SessionStoreImpl(connectionString);
        var storeResult = store.GetRecentSessions(query.MaxCount);
        if (!storeResult.Succeeded)
            return storeResult.ToResult<IReadOnlyList<SessionSummary>>();

        return MapSessions(storeResult.Value, query.Target);

        // ── helpers ──────────────────────────────────────────────────────────
        static Result<IReadOnlyList<SessionSummary>> MapSessions(
            IReadOnlyList<IJobSessionRecord> records, JrpTarget target)
        {
            var summaries = new List<SessionSummary>(records.Count);
            foreach (var r in records)
            {
                var summary = new SessionSummary
                {
                    Uid             = r.UID,
                    JobTypeName     = r.JobTypeName,
                    ExecutionStatus = ToSessionStatus(r.ExecutionStatus),
                    Started         = r.Started,
                    Finished        = r.Finished,
                    UserName        = r.UserName,
                    ClientMachine   = r.ClientMachine,
                    Environment     = target.Environment,
                    Machine         = target.Machine
                };
                summaries.Add(summary);
            }
            return Result<IReadOnlyList<SessionSummary>>.Success(summaries);
        }

        // Read-edge mapping: the platform-internal ExecutionStatus is translated to the
        // JRP-owned SessionStatus here so no backend type crosses the JRP boundary (DS-2 seam).
        static JrpStatus ToSessionStatus(BackendStatus status) => status switch
        {
            BackendStatus.Preparing                    => JrpStatus.Preparing,
            BackendStatus.Running                      => JrpStatus.Running,
            BackendStatus.RunningCancellationRequested => JrpStatus.RunningCancellationRequested,
            BackendStatus.FinishedSuccessfully         => JrpStatus.FinishedSuccessfully,
            BackendStatus.FinishedWithErrors           => JrpStatus.FinishedWithErrors,
            BackendStatus.FinishedFaulted              => JrpStatus.FinishedFaulted,
            BackendStatus.FinishedByCancellation       => JrpStatus.FinishedByCancellation,
            BackendStatus.FinishedRejected             => JrpStatus.FinishedRejected,
            BackendStatus.FinishedAbandoned            => JrpStatus.FinishedAbandoned,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unmapped ExecutionStatus.")
        };
    }
}
