using MeydanCleanApi.Template.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeydanCleanApi.Template.WebApi.Configurations.StartupChecks;

/// <summary>
/// Startup extension methods that refuse to let the API run in a state it cannot serve requests from.
/// </summary>
public static class StartupCheckExtensions
{
    /// <summary>
    /// Placeholder secrets that ship in this repository so a fresh clone runs without any setup.
    /// </summary>
    /// <remarks>
    /// These live in <c>appsettings.Development.json</c> and <c>.env.example</c>, which means they are
    /// public: anybody with the repository can sign a token with them. That is fine on a laptop and
    /// unacceptable anywhere else, so <see cref="EnsureProductionSecretsAreReal"/> refuses to
    /// start when one of them is still in place outside Development.
    /// </remarks>
    private static readonly string[] PlaceholderSecrets =
    [
        "Development_JwtSecurityKey_Minimum_32_Chars_Long_Secret_Key!",
        "Development_RefreshSecurityKey_Minimum_32_Chars_Long_Secret_Key!",
        "Development_StorageSigningKey_Minimum_32_Chars_Long_Secret_Key!",
        "Docker_Local_JwtSecurityKey_Minimum_32_Chars_Long_Key!",
        "Docker_Local_RefreshSecurityKey_Minimum_32_Chars_Long_Key!",
        "Docker_Local_StorageSigningKey_Minimum_32_Chars_Long_Key!",
    ];

    /// <summary>
    /// Configuration keys holding a secret that must be replaced before the API leaves Development.
    /// </summary>
    private static readonly string[] GuardedSecretKeys =
    [
        "Tokens:Jwt:JwtSecurityKey",
        "Tokens:Jwt:RefreshSecurityKey",
        "Storage:Local:SigningKey",
    ];

    /// <summary>
    /// Stops startup when a secret shipped for local development is still configured outside Development.
    /// </summary>
    /// <param name="app">The application host.</param>
    /// <exception cref="InvalidOperationException">Thrown when a placeholder secret is still in use.</exception>
    /// <remarks>
    /// The realistic accident is deploying with the environment left at its default, or copying
    /// <c>.env.example</c> to a server and never editing it. Both produce an API whose tokens anyone
    /// can forge, and neither looks broken from the outside, so the only safe response is to refuse
    /// to start and say which key is at fault.
    /// </remarks>
    public static void EnsureProductionSecretsAreReal(this IHost app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (environment.IsDevelopment())
        {
            return;
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var offendingKeys = GuardedSecretKeys
            .Where(key => configuration[key] is { } value && PlaceholderSecrets.Contains(value, StringComparer.Ordinal))
            .ToList();

        if (offendingKeys.Count > 0)
        {
            throw new InvalidOperationException(
                $"Refusing to start in the {environment.EnvironmentName} environment: " +
                $"{string.Join(", ", offendingKeys)} still hold a value published in this repository. " +
                "Generate replacements with 'openssl rand -base64 48' and supply them as environment " +
                "variables, using '__' for nesting (for example Tokens__Jwt__JwtSecurityKey).");
        }
    }

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
