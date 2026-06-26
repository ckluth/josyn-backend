using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Surface;
using JOSYN.Jrp.Surface.Queries;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Serves <see cref="GetJobArguments"/>: all argument records for one registered job.
/// </summary>
internal sealed class GetJobArgumentsHandler(string connectionString)
{
    internal Result<JobArguments> Handle(GetJobArguments query)
    {
        IJobRegistry registry = new SqlJobRegistry(connectionString);
        var storeResult = registry.GetByName(query.JobName);
        if (!storeResult.Succeeded)
            return JrpError.NotFound(
                $"Job '{query.JobName}' is not registered on this installation.");

        return Result<JobArguments>.Success(MapArguments(storeResult.Value, query));

        // ── helpers ──────────────────────────────────────────────────────────
        static JobArguments MapArguments(IJobRegistrationRecord record, GetJobArguments q) => new()
        {
            Environment       = q.Target.Environment,
            Machine           = q.Target.Machine,
            JobName           = record.Name,
            TechnicalUserName = record.TechnicalUserName,
            // Argument records are sorted by name so the listing is stable across calls.
            Arguments = record.ArgumentRecords
                .OrderBy(a => a.Name)
                .Select(a => new ArgumentSummary { Name = a.Name, Content = a.Content })
                .ToList()
        };
    }
}
