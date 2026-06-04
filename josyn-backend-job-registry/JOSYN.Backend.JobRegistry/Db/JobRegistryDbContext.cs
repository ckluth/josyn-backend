using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.JobRegistry;

internal sealed class JobRegistryDbContext(string connectionString) : DbContext
{
    public DbSet<JobRegistrationEntity> JobRegistrations => Set<JobRegistrationEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobRegistrationEntity>(e =>
        {
            e.ToTable("JobRegistry", "josyn");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.TechnicalUserName).IsRequired().HasMaxLength(256);
        });
    }
}
