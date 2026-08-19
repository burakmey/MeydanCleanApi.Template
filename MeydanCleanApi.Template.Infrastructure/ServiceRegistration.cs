using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Localization;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Abstractions.Tracing;
using MeydanCleanApi.Template.Infrastructure.Options.Authentication;
using MeydanCleanApi.Template.Infrastructure.Options.Tokens;
using MeydanCleanApi.Template.Infrastructure.Options.Storage;
using MeydanCleanApi.Template.Application.Abstractions.Clock;
using MeydanCleanApi.Template.Infrastructure.Services.Auth;
using MeydanCleanApi.Template.Infrastructure.Services.Auth.Tokens;
using MeydanCleanApi.Template.Infrastructure.Services.Auth.Verifiers;
using MeydanCleanApi.Template.Infrastructure.Services.Clock;
using MeydanCleanApi.Template.Infrastructure.Services.FileStorage;
using MeydanCleanApi.Template.Infrastructure.Services.FileStorage.Providers;
using MeydanCleanApi.Template.Infrastructure.Services.Localization;
using MeydanCleanApi.Template.Infrastructure.Services.Resolvers;
using MeydanCleanApi.Template.Infrastructure.Services.Tracing;
using MeydanCleanApi.Template.Infrastructure.Services.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeydanCleanApi.Template.Infrastructure;

/// <summary>
/// Dependency Injection extension methods for registering Infrastructure layer options and services.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers strongly-typed options and infrastructure service implementations with ASP.NET Core Dependency Injection.
    /// </summary>
    /// <param name="services">The ServiceCollection instance.</param>
    /// <param name="configuration">The application Configuration instance.</param>
    /// <returns>The ServiceCollection instance for method chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Core Framework Dependencies
        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        // 2. Bind Strongly-Typed Options Models using IOptionSection.SectionName
        // ValidateOnStart makes the app fail at boot when a required setting is missing or too weak,
        // instead of failing later on a real request.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(JwtOptions.IsValid, JwtOptions.ValidationMessage)
            .ValidateOnStart();

        services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));
        services.Configure<LocalStorageOptions>(configuration.GetSection(LocalStorageOptions.SectionName));
        services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));

        // 3. Register Tracing, Identity & Localization Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICorrelationIdContext, CorrelationIdContext>();
        services.AddSingleton(typeof(ILocalizationService<>), typeof(LocalizationService<>));

        // Scoped because it reads the SupportedCultures table through the scoped DbContext.
        // The result is cached in IMemoryCache, so the database is only hit once per cache window.
        services.AddScoped<ISupportedCultureProvider, SupportedCultureProvider>();

        // 4. Register Security & Auth Verifier Services
        services.AddScoped<ITokenService, JwtTokenService>();
        // Register one verifier per supported provider, then let the resolver index them.
        // Adding Apple or Microsoft sign-in means adding a line here and nothing else.
        services.AddScoped<IExternalAuthVerifier, GoogleAuthVerifier>();
        services.AddScoped<IExternalAuthVerifierResolver, ExternalAuthVerifierResolver>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IRefreshTokenDeliveryService, RefreshTokenCookieService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // IUserSessionService is registered by the Persistence layer, because storing sessions
        // needs the DbContext and Infrastructure must not depend on EF Core.

        // 5. Register Storage Services & Handler Resolvers
        // All four are Singleton on purpose. The providers only hold configuration and a cached
        // storage client, so there is nothing per-request to keep. Registering the providers as
        // Scoped while the resolver is Singleton would be a captive dependency and the app would
        // refuse to start.
        services.AddSingleton<IFileStorageService, LocalStorageService>();
        services.AddSingleton<IFileStorageService, SupabaseStorageService>();
        services.AddSingleton<IFileStorageHandlerResolver, FileStorageHandlerResolver>();
        services.AddSingleton<IFileStorageHandler, FileStorageHandler>();

        return services;
    }
}
