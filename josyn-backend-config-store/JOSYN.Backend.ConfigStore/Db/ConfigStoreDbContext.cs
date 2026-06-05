using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.ConfigStore;

internal sealed class ConfigStoreDbContext(string connectionString) : DbContext
{
    public DbSet<ConfigStoreEntity> ConfigStore => Set<ConfigStoreEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfigStoreEntity>(e =>
        {
            e.ToTable("ConfigStore", "josyn");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Key).IsRequired().HasMaxLength(256);
            e.Property(x => x.Value).IsRequired();
        });
    }
}
