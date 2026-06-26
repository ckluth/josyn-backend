using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <see cref="GetRegisteredJobs"/>: all registered jobs as a lightweight discovery listing.
/// </summary>
internal sealed class GetRegisteredJobsHandler(string connectionString)
{
    internal Result<IReadOnlyList<RegisteredJobSummary>> Handle(GetRegisteredJobs query)
    {
        IJobRegistry registry = new SqlJobRegistry(connectionString);
        var storeResult = registry.GetAll();
        if (!storeResult.Succeeded)
            return storeResult.ToResult<IReadOnlyList<RegisteredJobSummary>>();

        return Result<IReadOnlyList<RegisteredJobSummary>>.Success(
            MapJobs(storeResult.Value));

        // ── helpers ──────────────────────────────────────────────────────────
        // GetAll() already returns records ordered by name; preserve that ordering.
        static IReadOnlyList<RegisteredJobSummary> MapJobs(
            IReadOnlyList<IJobRegistrationRecord> records) =>
            records
                .Select(r => new RegisteredJobSummary
                {
                    JobName           = r.Name,
                    TechnicalUserName = r.TechnicalUserName,
                    ArgumentCount     = r.ArgumentRecords.Count
                })
                .ToList();
    }
}
