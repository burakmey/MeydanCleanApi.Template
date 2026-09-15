using MeydanCleanApi.Template.Application;
using MeydanCleanApi.Template.Infrastructure;
using MeydanCleanApi.Template.Persistence;
using MeydanCleanApi.Template.Persistence.Seeding;
using MeydanCleanApi.Template.WebApi.Configurations.HealthChecks;
using MeydanCleanApi.Template.WebApi.Configurations.Localization;
using MeydanCleanApi.Template.WebApi.Configurations.StartupChecks;
using MeydanCleanApi.Template.WebApi.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

namespace MeydanCleanApi.Template.WebApi;

/// <summary>
/// Application entry point: builds the host, checks the database, then wires the HTTP pipeline.
/// </summary>
public static class Program
{
    /// <summary>
    /// Starts the Web API.
    /// </summary>
    /// <param name="args">Command line arguments passed through to the host builder.</param>
    public static async Task Main(string[] args)
    {
        // ===================================================================================
        // STEP 1: INITIALIZE WEB APPLICATION BUILDER & SERILOG LOGGING ENGINE
        // ===================================================================================
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog to read settings from appsettings.json and write to Console/File
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MeydanCleanApi.Template");
        });

        // ===================================================================================
        // STEP 2: REGISTER CLEAN ARCHITECTURE LAYER SERVICES (DEPENDENCY INJECTION)
        // ===================================================================================

        // 1. Application Layer Services (Custom IMediator, FluentValidation, Pipeline Behaviors)
        builder.Services.AddApplicationServices();

        // 2. Infrastructure Layer Services (JWT Token Service, File Storage, OAuth Verifiers, Culture Provider)
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // 3. Persistence Layer Services (EF Core PostgreSQL DbContext, Identity, Repositories, Unit of Work)
        builder.Services.AddPersistenceServices(builder.Configuration);

        // 4. WebApi Presentation Layer Services (Controllers, Swagger, JWT Auth, CORS, Rate Limiting, Health Checks)
        builder.Services.AddWebApiServices(builder.Configuration);

        // ===================================================================================
        // STEP 3: BUILD WEB APPLICATION INSTANCE & PERFORM STARTUP CHECKS
        // ===================================================================================
        var app = builder.Build();

        // Refuse to run outside Development while a signing key published in this repository is still
        // configured. Checked before anything else, because an API that starts with a public key looks
        // perfectly healthy while anybody can forge a token for it.
        app.EnsureProductionSecretsAreReal();

        // Verify the database is reachable and that the schema has been migrated.
        // Startup stops here if the database cannot be used, so the API never reports healthy while broken.
        await app.PerformStartupChecksAsync();

        // Insert reference data and the bootstrap administrator account.
        // Safe to run on every start: each seeder checks whether its data already exists.
        await app.Services.SeedDatabaseAsync();

        // ===================================================================================
        // STEP 4: CONFIGURE HTTP REQUEST MIDDLEWARE PIPELINE (EXECUTION ORDER MATTERS!)
        // ===================================================================================

        // 1. Forwarded Headers: behind a reverse proxy or load balancer this restores the real client IP
        //    and original scheme. Rate limiting partitions by client IP, so this has to run first.
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        // 2. CorrelationId Middleware: Extracts or generates X-Correlation-ID header, tags Serilog LogContext, and sets response header
        app.UseCorrelationId();

        // 3. Request Localization: Reads active cultures from DB/Cache at startup and sets thread culture (tr-TR, en-US)
        await app.UseApplicationRequestLocalizationAsync();

        // 4. Serilog Request Logging: Logs HTTP request method, path, status code, and execution duration
        app.UseSerilogRequestLogging();

        // 5. Global Exception Handler: Intercepts uncaught exceptions and converts them to a localized JSON error payload
        app.UseExceptionHandler();

        // 6. HTTPS Redirection: Redirects HTTP requests to secure HTTPS endpoints
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        // 7. Static Files: Serves public assets from wwwroot.
        //    Uploaded files are deliberately NOT stored here. See Storage:Local:RootDirectory in appsettings.json:
        //    private uploads must never be reachable without going through an authorized endpoint.
        app.UseStaticFiles();

        // 8. Swagger API Documentation: Renders Swagger UI interactive documentation in Development mode
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meydan Clean API v1");
                c.RoutePrefix = "swagger";
            });
        }

        // 9. Routing: Matches incoming HTTP request URL to target Controller action route
        app.UseRouting();

        // 10. CORS: Controls Cross-Origin Resource Sharing policy for Web and Mobile clients
        app.UseCors(ServiceRegistration.CorsPolicyName);

        // 11. Rate Limiting: Throttles abusive clients. Runs before authentication so that unauthenticated
        //     floods (for example password guessing against /login) are rejected as early as possible.
        app.UseRateLimiter();

        // 12. Authentication: Validates User Identity (e.g. checks JWT Bearer token signature, expiration, and session status)
        app.UseAuthentication();

        // 13. Authorization: Validates User Permissions (e.g. checks if user meets [Authorize] roles/policies)
        app.UseAuthorization();

        // 14. Map Endpoints: Maps Controller routes and Health Check endpoints
        app.MapControllers();

        // /health public liveness, /health/ready public readiness, /health/details admin only.
        app.MapApplicationHealthChecks();

        // ===================================================================================
        // STEP 5: RUN THE WEB APPLICATION
        // ===================================================================================
        await app.RunAsync();
    }
}
