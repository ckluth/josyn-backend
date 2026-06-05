using JOSYN.Backend.AdapterContracts;
using JOSYN.Foundation.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.ConfigStore;

/// <inheritdoc cref="IConfigStore"/>
public sealed class ConfigStore(IConfigSource source, string connectionString) : IConfigStore
{
    /// <inheritdoc/>
    public Result<string> GetValue(string key) => source.GetValue(key);

    /// <inheritdoc/>
    public Result SetValue(string key, string value)
    {
        try
        {
            using var ctx = new ConfigStoreDbContext(connectionString);
            var entity = ctx.ConfigStore.FirstOrDefault(e => e.Key == key);

            if (entity is null)
                ctx.ConfigStore.Add(new ConfigStoreEntity { Key = key, Value = value });
            else
                entity.Value = value;

            ctx.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }
}
