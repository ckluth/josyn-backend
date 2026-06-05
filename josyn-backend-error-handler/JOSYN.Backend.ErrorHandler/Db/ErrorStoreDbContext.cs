using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.ErrorHandler;

internal sealed class ErrorStoreDbContext(string connectionString) : DbContext
{
    public DbSet<ErrorStoreEntity> ErrorStore => Set<ErrorStoreEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErrorStoreEntity>(e =>
        {
            e.ToTable("ErrorStore", "josyn");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.UID).IsRequired();
            e.Property(x => x.OccurredAt).IsRequired();
            e.Property(x => x.Causer).IsRequired().HasMaxLength(256);
            e.Property(x => x.Message).IsRequired();
            e.Property(x => x.JobName).HasMaxLength(256);
        });
    }
}
