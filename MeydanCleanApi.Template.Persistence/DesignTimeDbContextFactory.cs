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

        return new ApplicationDbContext(builder.Options);
    }
}
