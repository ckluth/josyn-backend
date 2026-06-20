using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.JobScheduleStore;

internal sealed class FiredSlotStoreDbContext(string connectionString) : DbContext
{
    public DbSet<FiredSlotEntity> FiredSlots => Set<FiredSlotEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FiredSlotEntity>(e =>
        {
            e.ToTable("FiredSlots", "josyn");
            e.HasKey(x => new { x.JobName, x.ArgumentRecordName, x.SlotTime });
            e.Property(x => x.JobName).IsRequired().HasMaxLength(256);
            e.Property(x => x.ArgumentRecordName).IsRequired().HasMaxLength(128);
            e.Property(x => x.SlotTime).IsRequired().HasColumnType("datetime2");
            e.Property(x => x.FiredAt).IsRequired().HasColumnType("datetime2");
        });
    }
}
