using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.JobRegistry;

public sealed class SqlJobRegistry(string connectionString) : IJobRegistry
{
    public Result<IJobRegistrationRecord> GetByName(string name)
    {
        try
        {
            using var ctx = new JobRegistryDbContext(connectionString);
            var entity = ctx.JobRegistrations
                .AsNoTracking()
                .Include(e => e.ArgumentRecords)
                .FirstOrDefault(e => e.Name == name);

            if (entity is null)
                return Result.Error($"No job registration found for name '{name}'.");

            return ToRecord(entity);
        }
        catch (Exception ex) { return ex; }
    }

    public Result<IReadOnlyList<IJobRegistrationRecord>> GetAll()
    {
        try
        {
            using var ctx = new JobRegistryDbContext(connectionString);
            var entities = ctx.JobRegistrations
                .AsNoTracking()
                .Include(e => e.ArgumentRecords)
                .OrderBy(e => e.Name)
                .ToList();

            IReadOnlyList<IJobRegistrationRecord> records = entities
                .Select(e => (IJobRegistrationRecord)ToRecord(e))
                .ToList();

            return Result<IReadOnlyList<IJobRegistrationRecord>>.Success(records);
        }
        catch (Exception ex) { return ex; }
    }

    public Result<IArgumentRecord> GetArgument(string jobName, string argumentName)
    {
        try
        {
            using var ctx = new JobRegistryDbContext(connectionString);
            var entity = ctx.ArgumentRecords
                .AsNoTracking()
                .FirstOrDefault(e => e.JobName == jobName && e.Name == argumentName);

            if (entity is null)
                return Result.Error($"No argument record '{argumentName}' found for job '{jobName}'.");

            return new ArgumentRecord
            {
                JobName = entity.JobName,
                Name    = entity.Name,
                Content = entity.Content
            };
        }
        catch (Exception ex) { return ex; }
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    private static JobRegistrationRecord ToRecord(JobRegistrationEntity entity) =>
        new()
        {
            Name              = entity.Name,
            TechnicalUserName = entity.TechnicalUserName,
            ArgumentRecords   = entity.ArgumentRecords
                .Select(a => (IArgumentRecord)new ArgumentRecord
                {
                    JobName = a.JobName,
                    Name    = a.Name,
                    Content = a.Content
                })
                .ToList()
        };
}
