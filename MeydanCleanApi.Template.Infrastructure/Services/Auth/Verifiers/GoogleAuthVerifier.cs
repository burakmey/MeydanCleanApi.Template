using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Common.Models.Auth;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Common.Constants;
using MeydanCleanApi.Template.Infrastructure.Common.Extensions;
using MeydanCleanApi.Template.Infrastructure.Options.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace MeydanCleanApi.Template.Infrastructure.Services.Auth.Verifiers;

/// <summary>
/// Verifies Google external OAuth2 ID tokens and extracts verified user profiles.
/// </summary>
/// <remarks>
/// Configuration is checked when a token is actually verified, not in the constructor. Every
/// registered verifier is constructed whenever the auth service is resolved, so throwing here
/// would break ordinary email and password login for anyone who has not set up Google sign-in.
/// </remarks>
public sealed class GoogleAuthVerifier : IExternalAuthVerifier
{
    private readonly GoogleOptions _googleOptions;
    private readonly Lazy<ConfigurationManager<OpenIdConnectConfiguration>> _configManager;

    /// <inheritdoc />
    public AuthProviderType AuthProvider => AuthProviderType.Google;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleAuthVerifier"/> class.
    /// </summary>
    /// <param name="options">Google authentication settings option.</param>
    public GoogleAuthVerifier(IOptions<GoogleOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _googleOptions = options.Value;

        // Lazy so the metadata endpoint is only contacted the first time a Google token arrives.
        _configManager = new Lazy<ConfigurationManager<OpenIdConnectConfiguration>>(() =>
            new ConfigurationManager<OpenIdConnectConfiguration>(
                _googleOptions.MetaData,
                new OpenIdConnectConfigurationRetriever()));
    }

    /// <inheritdoc />
    public async Task<ExternalUserModel> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw InvalidCredentialsException.WithCode();
        }

        EnsureConfigured();

        OpenIdConnectConfiguration config;
        try
        {
            config = await _configManager.Value.GetConfigurationAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw ConfigurationMissingException.ForSection(GoogleOptions.SectionName, nameof(GoogleOptions.MetaData));
        }

        TokenValidationResult result;
        try
        {
            var validationParams = new TokenValidationParameters
            {
                ValidIssuer = _googleOptions.Issuer,
                ValidAudience = _googleOptions.ClientId,
                IssuerSigningKeys = config.SigningKeys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            var handler = new JsonWebTokenHandler();
            result = await handler.ValidateTokenAsync(idToken, validationParams);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw InvalidCredentialsException.WithCode();
        }

        if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt)
        {
            throw InvalidCredentialsException.WithCode();
        }

        try
        {
            return new ExternalUserModel
            {
                AuthProvider = AuthProvider,
                Subject = jwt.GetRequiredClaimValue(JwtClaimTypes.Subject),
                Email = jwt.GetRequiredClaimValue(JwtClaimTypes.Email),
                Name = jwt.GetRequiredClaimValue(JwtClaimTypes.Name),
                EmailVerified = bool.TryParse(jwt.GetClaimValue(JwtClaimTypes.EmailVerified), out var verified) && verified
            };
        }
        catch (SecurityTokenException)
        {
            throw InvalidCredentialsException.WithCode();
        }
    }

    /// <summary>
    /// Throws a clear configuration error if Google sign-in has not been set up.
    /// </summary>
    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_googleOptions.ClientId))
            throw ConfigurationMissingException.ForSection(GoogleOptions.SectionName, nameof(GoogleOptions.ClientId));

        if (string.IsNullOrWhiteSpace(_googleOptions.Issuer))
            throw ConfigurationMissingException.ForSection(GoogleOptions.SectionName, nameof(GoogleOptions.Issuer));

        if (string.IsNullOrWhiteSpace(_googleOptions.MetaData))
            throw ConfigurationMissingException.ForSection(GoogleOptions.SectionName, nameof(GoogleOptions.MetaData));
    }
}
