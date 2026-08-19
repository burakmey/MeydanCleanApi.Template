using System.Text;
using MeydanCleanApi.Template.Infrastructure.Common.Constants;
using MeydanCleanApi.Template.Infrastructure.Options.Tokens;
using MeydanCleanApi.Template.Application.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MeydanCleanApi.Template.WebApi.Configurations.JwtBearer;

/// <summary>
/// Configures ASP.NET Core <see cref="JwtBearerOptions"/> dynamically using strongly-typed <see cref="JwtOptions"/> (Section: <c>Tokens:Jwt</c>).
/// Handles token validation parameters, inbound claim mapping, and localized 401/403 event responses with zero hardcoded strings.
/// </summary>
public sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string AuthFailureMessageKey = "__auth_failure_message";
    private readonly JwtOptions _jwtOptions;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtBearerOptionsSetup"/> class.
    /// </summary>
    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(jwtOptions);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _jwtOptions = jwtOptions.Value;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }

    /// <inheritdoc />
    public void Configure(JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Turn off legacy inbound claim mapping to preserve original JWT claim short names (sub, jti, email, role)
        options.MapInboundClaims = false;

        // Enforce modern JsonWebTokenHandler instead of legacy JwtSecurityTokenHandler
        options.UseSecurityTokenValidators = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.JwtSecurityKey)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero, // Strict token expiry with zero grace period

            // These must match the claim names the token is actually written with
            // (see JwtTokenService and JwtClaimTypes). Because MapInboundClaims is off above,
            // claims keep their short names, so pointing RoleClaimType at the long
            // ClaimTypes.Role URI would mean [Authorize(Roles = ...)] never matches anything.
            NameClaimType = JwtClaimTypes.Email,
            RoleClaimType = JwtClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            // Sanity check on the claims this API relies on. The signature has already been verified
            // at this point, so this only guards against tokens that are valid but malformed.
            //
            // Note: this does not check whether the session was logged out. Doing so would mean a
            // database read on every request. Instead, access tokens are short lived and logout
            // deletes the refresh token, so a signed-out user cannot get a new one.
            OnTokenValidated = context =>
            {
                var claims = context.Principal?.Claims?.ToList();
                if (claims is null)
                {
                    context.Fail(ErrorCodes.Unauthorized);
                    return Task.CompletedTask;
                }

                var subClaim = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject)?.Value;
                var jtiClaim = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Jti)?.Value;

                if (string.IsNullOrWhiteSpace(subClaim) || !Guid.TryParse(subClaim, out _))
                {
                    context.Fail(ErrorCodes.Unauthorized);
                    return Task.CompletedTask;
                }

                if (string.IsNullOrWhiteSpace(jtiClaim))
                {
                    context.Fail(ErrorCodes.Unauthorized);
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                context.HttpContext.Items[AuthFailureMessageKey] = ErrorCodes.Unauthorized;
                return Task.CompletedTask;
            },

            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted) return;

                using var scope = _serviceProvider.CreateScope();
                var localizer = scope.ServiceProvider.GetRequiredService<ILocalizationService<ErrorMessages>>();

                var errorCode = context.HttpContext.Items[AuthFailureMessageKey] as string ?? ErrorCodes.Unauthorized;
                var localizedMessage = localizer.Get(errorCode);

                await WriteJsonResponseAsync(context.Response, StatusCodes.Status401Unauthorized, localizedMessage, errorCode);
            },

            OnForbidden = async context =>
            {
                if (context.Response.HasStarted) return;

                using var scope = _serviceProvider.CreateScope();
                var localizer = scope.ServiceProvider.GetRequiredService<ILocalizationService<ErrorMessages>>();

                var errorCode = ErrorCodes.Forbidden;
                var localizedMessage = localizer.Get(errorCode);

                await WriteJsonResponseAsync(context.Response, StatusCodes.Status403Forbidden, localizedMessage, errorCode);
            }
        };
    }

    private static async Task WriteJsonResponseAsync(
        HttpResponse response,
        int statusCode,
        string message,
        string errorCode)
    {
        response.StatusCode = statusCode;

        // WriteAsJsonAsync, not JsonSerializer.Serialize. Serialize would use the plain .NET defaults
        // and emit PascalCase here while every other error comes back camelCase, so a client would need
        // two ways to read the same ErrorResponse. This picks up the application's JSON settings.
        await response.WriteAsJsonAsync(new ErrorResponse
        {
            Status = statusCode,
            ErrorCode = errorCode,
            Message = message,
            Errors = null
        });
    }
}
