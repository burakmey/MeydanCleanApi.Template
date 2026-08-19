using System.Globalization;
using System.Threading.RateLimiting;
using MeydanCleanApi.Template.Application.Localization;
using MeydanCleanApi.Template.Persistence.Contexts;
using MeydanCleanApi.Template.WebApi.Configurations.Authorization;
using MeydanCleanApi.Template.WebApi.Configurations.HealthChecks;
using MeydanCleanApi.Template.WebApi.Configurations.JwtBearer;
using MeydanCleanApi.Template.WebApi.Configurations.Swagger;
using MeydanCleanApi.Template.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeydanCleanApi.Template.WebApi;

/// <summary>
/// Dependency Injection extension methods for registering Presentation (WebApi) layer services.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>Name of the CORS policy applied to every endpoint.</summary>
    public const string CorsPolicyName = "DefaultCorsPolicy";

    /// <summary>Name of the rate limiting policy applied to authentication endpoints.</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Configuration key holding the array of browser origins allowed to call this API.</summary>
    private const string CorsOriginsKey = "CorsOrigins";

    /// <summary>
    /// Registers Web API services including controllers, RFC 7807 exception handler, JWT authentication setups, and Swagger.
    /// </summary>
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Register Controllers & JSON Serializer Settings
        // [ApiController] rejects a malformed body before the request ever reaches a handler, and by
        // default it answers with ProblemDetails. That is a second error shape, so a client would need
        // two deserializers: one for this and one for every other failure. Rebuild it as ErrorResponse
        // so a missing JSON field looks exactly like a FluentValidation failure.
        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var localizer = context.HttpContext.RequestServices
                    .GetRequiredService<ILocalizationService<ErrorMessages>>();

                var errors = context.ModelState
                    .Where(entry => entry.Value is not null && entry.Value.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                // Only the wrapper is translated. The per-field text comes from the model binder, so it
                // stays in English; anything with its own validator is translated by ValidationBehavior.
                return new BadRequestObjectResult(new ErrorResponse
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = ErrorCodes.Validation,
                    Message = localizer.Get(ErrorCodes.Validation),
                    Errors = errors
                });
            };
        });

        // 2. Register Global RFC 7807 Exception Handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // 3. Register .resx Localization
        // Without this, IStringLocalizer<T> is not registered and every handler asking for
        // ILocalizationService<T> fails to resolve.
        //
        // No ResourcesPath is set on purpose. IStringLocalizer<T> then looks for a .resx whose name
        // matches the marker type's full name, which means each file sits directly beside the class
        // it belongs to in Application/Localization. Setting a path here would send it looking in a
        // separate folder tree instead.
        services.AddLocalization();

        // 4. Register Health Checks
        // Backs /health/ready, so the endpoint reports Unhealthy when the database cannot serve a
        // request, instead of reporting Healthy while every query fails.
        //
        // customTestQuery matters. Without it, AddDbContextCheck only opens a connection, which
        // succeeds against a completely empty database, and readiness reports Healthy while no table
        // exists. Counting a seeded lookup table proves the schema was actually created.
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: [HealthCheckExtensions.ReadyTag],
                customTestQuery: async (dbContext, ct) =>
                {
                    // Throws when the table is missing, which the health check reports as Unhealthy.
                    // The row count itself is irrelevant, so an empty table still counts as healthy.
                    _ = await dbContext.SupportedCultures.CountAsync(ct);
                    return true;
                });

        // 5. Configure Authentication (JWT Bearer Scheme Setup)
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

        // 6. Configure Application Authorization Policies (SuperAdmin, Admin, Customer)
        services.AddApplicationAuthorization();

        // 7. Configure CORS from the CorsOrigins array in appsettings.json
        services.AddApplicationCors(configuration);

        // 8. Configure Rate Limiting
        services.AddApplicationRateLimiting();

        // 9. Configure Swagger/OpenAPI Generator with EndpointsApiExplorer
        services.AddSwaggerDocumentation();

        return services;
    }

    /// <summary>
    /// Registers the CORS policy using the origins listed under the <c>CorsOrigins</c> configuration key.
    /// </summary>
    /// <remarks>
    /// Browser clients that send the refresh token cookie need <c>AllowCredentials</c>, and ASP.NET Core
    /// forbids combining that with <c>AllowAnyOrigin</c>. So the allowed origins must be listed explicitly.
    /// If no origins are configured the policy allows nothing, which fails closed rather than open.
    /// </remarks>
    private static IServiceCollection AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection(CorsOriginsKey).Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers rate limiting: a global per-client limit plus a stricter limit for authentication endpoints.
    /// </summary>
    /// <remarks>
    /// The auth policy exists to slow down password guessing. Clients are identified by IP address,
    /// so remember to add <c>UseForwardedHeaders</c> when running behind a reverse proxy, otherwise
    /// every request looks like it comes from the proxy.
    /// </remarks>
    private static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global limit: 100 requests per minute per client.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolveClientKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Stricter limit for login/refresh/external-login: 10 attempts per minute per client.
            options.AddPolicy(AuthRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolveClientKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Without this a rejected request returns 429 with an empty body, which is the one error in
            // the API a client could not read. Answer with the same ErrorResponse as everything else.
            options.OnRejected = async (context, cancellationToken) =>
            {
                var localizer = context.HttpContext.RequestServices
                    .GetRequiredService<ILocalizationService<ErrorMessages>>();

                // Tell the caller when it is worth retrying, so clients can back off instead of hammering.
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    ErrorCode = ErrorCodes.TooManyRequests,
                    Message = localizer.Get(ErrorCodes.TooManyRequests)
                }, cancellationToken);
            };
        });

        return services;
    }

    private static string ResolveClientKey(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
