using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrainingGoal.Api.Models;

namespace TrainingGoal.Api.Data;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<LogEntry> Logs => Set<LogEntry>();

    // Stores ASP.NET Core Data Protection keys so the auth cookie survives container restarts.
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogEntry>()
            .HasOne(l => l.Goal)
            .WithMany(g => g.Logs)
            .HasForeignKey(l => l.GoalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LogEntry>()
            .HasIndex(l => new { l.GoalId, l.Date });
    }
}
