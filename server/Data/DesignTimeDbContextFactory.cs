using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrainingGoal.Api.Data;

/// <summary>
/// Used only by the EF Core CLI (dotnet ef ...) so migrations can be created without
/// running the app or connecting to a real database.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
