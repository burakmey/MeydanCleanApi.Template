using MeydanCleanApi.Template.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeydanCleanApi.Template.WebApi.Configurations.StartupChecks;

/// <summary>
/// Startup extension methods that verify the database is reachable and the schema is up to date.
/// </summary>
public static class StartupCheckExtensions
{
    /// <summary>
    /// Verifies database connectivity and applies any pending EF Core migrations.
    /// </summary>
    /// <remarks>
    /// This deliberately throws instead of only logging. An API that starts without a usable database
    /// reports itself as healthy and then fails every request, which is much harder to diagnose than
    /// a clear crash at boot.
    /// </remarks>
    /// <param name="app">The application host.</param>
    /// <exception cref="InvalidOperationException">Thrown when the database cannot be reached.</exception>
    public static async Task PerformStartupChecksAsync(this IHost app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHost>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Performing database connection startup check...");

        if (!await dbContext.Database.CanConnectAsync())
        {
            throw new InvalidOperationException(
                "Cannot connect to the database. Check that PostgreSQL is running and that " +
                "ConnectionStrings:DefaultConnection points at it.");
        }

        logger.LogInformation("Database connection successful.");

        // The repository ships an InitialCreate migration, so a clone runs immediately. A project
        // created with "dotnet new" does not receive one: it has to match the options chosen there
        // and the entities of that domain, so it is generated from that model rather than inherited.
        // The warning below names the exact command for that case.
        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
        var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList();

        if (pendingMigrations.Count == 0 && appliedMigrations.Count == 0)
        {
            logger.LogWarning(
                "No EF Core migrations were found. The database schema has not been created. Run:\n" +
                "  dotnet ef migrations add InitialCreate --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi\n" +
                "  dotnet ef database update --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi");
            return;
        }

        if (pendingMigrations.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
    }
}
