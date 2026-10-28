using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitTrack.Api.Data;

/// <summary>
/// Used only by "dotnet ef" so migrations can be created even when the app is set to
/// DataSource = Json (in that mode the DbContext is not registered at all).
/// Creating a migration does not connect to the database.
/// </summary>
public class WorkoutDbContextFactory : IDesignTimeDbContextFactory<WorkoutDbContext>
{
    public WorkoutDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<WorkoutDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("FitTrack")
            ?? "Server=localhost,1433;Database=FitTrack;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<WorkoutDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new WorkoutDbContext(options);
    }
}
