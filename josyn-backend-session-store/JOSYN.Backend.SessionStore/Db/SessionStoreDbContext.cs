using Microsoft.EntityFrameworkCore;

namespace JOSYN.Backend.SessionStore;

internal sealed class SessionStoreDbContext(string connectionString) : DbContext
{
    public DbSet<SessionStoreEntity> SessionStore => Set<SessionStoreEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionStoreEntity>(e =>
        {
            e.ToTable("SessionStore", "josyn");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.UID).IsRequired();
            e.Property(x => x.JobTypeName).IsRequired().HasMaxLength(256);
            e.Property(x => x.Arguments).IsRequired();
            e.Property(x => x.Result).IsRequired();
            e.Property(x => x.JobVersion).IsRequired().HasMaxLength(24).IsUnicode(false);
            e.Property(x => x.UserName).IsRequired().HasMaxLength(64).IsUnicode(false);
            e.Property(x => x.UserDomain).IsRequired().HasMaxLength(32).IsUnicode(false);
            e.Property(x => x.ClientApplication).IsRequired().HasMaxLength(128).IsUnicode(false);
            e.Property(x => x.ClientMachine).IsRequired().HasMaxLength(64).IsUnicode(false);
            e.Property(x => x.TecUser).HasMaxLength(64).IsUnicode(false);
            e.Property(x => x.Started).IsRequired();
            e.Property(x => x.ExecutionStatus).IsRequired().HasMaxLength(32).IsUnicode(false);
            e.Property(x => x.Progress).HasMaxLength(512).IsUnicode(false);
            e.Property(x => x.JapServerProcess).IsRequired();
            e.Property(x => x.JobHostProcessId).IsRequired();
            e.Property(x => x.JapExitCode).IsRequired();
            e.Property(x => x.JobExitCode).IsRequired();
            e.Property(x => x.WrittenBy).HasMaxLength(64).IsUnicode(false);
        });
    }
}
