namespace BlazorApp1;

using Microsoft.EntityFrameworkCore;

public class TimeTrackingDbContext : DbContext
{
    public TimeTrackingDbContext(DbContextOptions<TimeTrackingDbContext> options)
        : base(options)
    {
    }

    // Your table(s)
    public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

    // If you have a Users table:
    // public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.ToTable("TimeEntries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WindowTitle).HasMaxLength(500);
            entity.Property(e => e.ProcessName).HasMaxLength(100);

            // If referencing a Users table, set up the foreign key:
            // entity.HasOne<User>()
            //       .WithMany()
            //       .HasForeignKey(e => e.UserId);
        });
    }
}