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
                .FirstOrDefault(e => e.Name == name);

            if (entity is null)
                return Result.Error($"No job registration found for name '{name}'.");

            return new JobRegistrationRecord
            {
                Name              = entity.Name,
                TechnicalUserName = entity.TechnicalUserName
            };
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
                .OrderBy(e => e.Name)
                .ToList();

            IReadOnlyList<IJobRegistrationRecord> records = entities
                .Select(e => (IJobRegistrationRecord)new JobRegistrationRecord
                {
                    Name              = e.Name,
                    TechnicalUserName = e.TechnicalUserName
                })
                .ToList();

            return Result<IReadOnlyList<IJobRegistrationRecord>>.Success(records);
        }
        catch (Exception ex) { return ex; }
    }
}
