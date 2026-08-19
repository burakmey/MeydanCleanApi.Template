using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Persistence.Contexts;
using MeydanCleanApi.Template.Persistence.Repositories;
using MeydanCleanApi.Template.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeydanCleanApi.Template.Persistence;

/// <summary>
/// Extension methods for registering EF Core DbContext, Repositories, UnitOfWork, and Identity with ASP.NET Core DI.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Adds Persistence layer services, DbContext, ASP.NET Core Identity, generic repositories, and Unit of Work to DI.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The service collection instance for method chaining.</returns>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw ConfigurationMissingException.ForSection("ConnectionStrings", "DefaultConnection");
        }

        // 1. Configure EF Core DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

                // Retries short-lived network problems instead of failing the request. Managed
                // PostgreSQL services drop idle connections routinely, so this matters in production.
                b.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            });
        });

        // 2. Configure ASP.NET Core Identity
        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // 3. Register Repositories and Unit of Work
        services.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(WriteRepository<,>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 4. Register persistence-backed application services
        // Session storage lives here rather than in Infrastructure because it needs the DbContext.
        services.AddScoped<IUserSessionService, UserSessionService>();

        return services;
    }
}
