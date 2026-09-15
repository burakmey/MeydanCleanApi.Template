using MeydanCleanApi.Template.Application.Abstractions.Clock;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MeydanCleanApi.Template.Persistence;

/// <summary>
/// Design-time factory for <see cref="ApplicationDbContext"/> enabling EF Core CLI tooling (<c>dotnet ef migrations add</c>).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <inheritdoc />
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // Locate Web API project directory or fallback to current directory
        var apiPath = Path.Combine(basePath, "..", "MeydanCleanApi.Template.WebApi");
        if (!Directory.Exists(apiPath))
        {
            apiPath = basePath;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw ConfigurationMissingException.ForSection("ConnectionStrings", "DefaultConnection");
        }

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(builder.Options, new SystemClock());
    }

    /// <summary>
    /// Clock handed to the context when the EF Core CLI builds it.
    /// </summary>
    /// <remarks>
    /// The tooling only reads the model to produce a migration; it never saves, so this is never asked
    /// for the time. It exists because the context takes a clock at runtime, and there is no dependency
    /// injection container here to supply the real one.
    /// </remarks>
    private sealed class SystemClock : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
