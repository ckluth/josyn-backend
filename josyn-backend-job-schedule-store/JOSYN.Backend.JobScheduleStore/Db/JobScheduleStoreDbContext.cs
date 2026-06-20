using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.JobScheduleStore;

internal sealed class JobScheduleStoreDbContext(string connectionString) : DbContext
{
    public DbSet<JobScheduleEntity>      JobSchedules      => Set<JobScheduleEntity>();
    public DbSet<JobScheduleEntryEntity> JobScheduleEntries => Set<JobScheduleEntryEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobScheduleEntity>(e =>
        {
            e.ToTable("JobSchedules", "josyn");
            e.HasKey(x => x.JobName);
            e.Property(x => x.JobName).IsRequired().HasMaxLength(256);
            e.Property(x => x.Suspended).IsRequired();
            e.Property(x => x.SuspendedUntil).HasColumnType("date");
        });

        modelBuilder.Entity<JobScheduleEntryEntity>(e =>
        {
            e.ToTable("JobScheduleEntries", "josyn");
            e.HasKey(x => new { x.JobName, x.ArgumentRecordName });
            e.Property(x => x.JobName).IsRequired().HasMaxLength(256);
            e.Property(x => x.ArgumentRecordName).IsRequired().HasMaxLength(256);
            e.Property(x => x.ScheduleDefinition).IsRequired();

            e.HasOne(x => x.Schedule)
             .WithMany(x => x.Entries)
             .HasForeignKey(x => x.JobName);
        });
    }
}
