using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Seeding;

/// <summary>
/// Provides extension methods for executing database seeders on application startup.
/// </summary>
public static class DatabaseSeederExtensions
{
    /// <summary>
    /// Executes all registered database seeders in dependency order within an isolated DI scope.
    /// </summary>
    /// <param name="services">The root service provider used to open an async scope.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the async seeding process.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Order matters:</strong> lookup tables and system roles first, then the bootstrap
    /// SuperAdmin account, and finally the sample catalog data.
    /// </para>
    /// <para>
    /// <strong>Safe to run repeatedly:</strong> every seeder checks whether its data already exists.
    /// </para>
    /// <para>
    /// <strong>Failure handling:</strong> outside development a failure stops startup. Continuing
    /// would leave the database half filled while the API reports itself healthy, which is much
    /// harder to notice than a crash.
    /// </para>
    /// </remarks>
    public static async Task SeedDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseSeeder");
        var environment = provider.GetRequiredService<IHostEnvironment>();

        try
        {
            logger.LogInformation("Starting database seeding sequence...");

            var context = provider.GetRequiredService<ApplicationDbContext>();
            var roleManager = provider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = provider.GetRequiredService<UserManager<AppUser>>();
            var configuration = provider.GetRequiredService<IConfiguration>();

            // 1. Seed lookup reference data (FileStatus, FileStorage, AuthProvider, SupportedCulture)
            await LookupDataSeederService.SeedAsync(context, logger, ct);

            // 2. Seed system roles & SuperAdmin account
            await AdminUserSeederService.SeedAsync(roleManager, userManager, context, configuration, logger, ct);

            //#if (IncludeSamples)
            // 3. Seed sample catalog data. Demo rows have no place in a real environment.
            if (environment.IsDevelopment())
            {
                var sampleProductLogger = loggerFactory.CreateLogger<SampleProductSeederService>();
                await new SampleProductSeederService(context, sampleProductLogger).SeedAsync(ct);
            }
            //#endif

            logger.LogInformation("Database seeding sequence completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seeding failed.");

            if (!environment.IsDevelopment())
            {
                throw;
            }

            logger.LogWarning("Continuing startup because the environment is Development.");
        }
    }
}
