using JOSYN.Backend.AdapterContracts;
using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.ConfigStore;

/// <summary>
/// Built-in <see cref="IConfigSource"/> implementation.
/// Reads configuration values from the <c>josyn.ConfigStore</c> SQL table.
/// </summary>
public sealed class SqlConfigSource(string connectionString) : IConfigSource
{
    /// <inheritdoc/>
    public Result<string> GetValue(string key)
    {
        try
        {
            using var ctx = new ConfigStoreDbContext(connectionString);
            var entity = ctx.ConfigStore
                .AsNoTracking()
                .FirstOrDefault(e => e.Key == key);

            if (entity is null)
                return Result.Error($"Konfigurationsschlüssel nicht gefunden: '{key}'");

            return entity.Value;
        }
        catch (Exception ex) { return ex; }
    }
}
