using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Clock;
using MeydanCleanApi.Template.Application.Common.Models.Token;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Common.Constants;
using MeydanCleanApi.Template.Infrastructure.Options.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MeydanCleanApi.Template.Infrastructure.Services.Auth.Tokens;

/// <summary>
/// Infrastructure JWT implementation of <see cref="ITokenService"/>.
/// Generates signed access tokens, cryptographically secure refresh tokens, and parses expired tokens.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IDateTimeService _dateTimeService;
    private readonly JsonWebTokenHandler _tokenHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="jwtOptions">Injected JWT options model.</param>
    /// <param name="dateTimeService">Clock abstraction used for token expiry.</param>
    public JwtTokenService(IOptions<JwtOptions> jwtOptions, IDateTimeService dateTimeService)
    {
        ArgumentNullException.ThrowIfNull(jwtOptions);
        ArgumentNullException.ThrowIfNull(dateTimeService);

        _jwtOptions = jwtOptions.Value;
        _dateTimeService = dateTimeService;
        _tokenHandler = new JsonWebTokenHandler();
    }

    /// <inheritdoc />
    public TokenModel CreateTokens(Guid userId, string email, IEnumerable<string> roles)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        var now = _dateTimeService.UtcNow;
        var jti = Guid.NewGuid().ToString("N");
        var accessTokenExpiresAt = now.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);
        var refreshTokenExpiresAt = now.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.JwtSecurityKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, userId.ToString()),
            new(JwtClaimTypes.Email, email),
            new(JwtClaimTypes.Jti, jti)
        };

        if (roles != null)
        {
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = accessTokenExpiresAt,
            SigningCredentials = credentials
        };

        var accessToken = _tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = CreateRefreshToken();

        return new TokenModel
        {
            AccessToken = accessToken,
            AccessTokenExpiration = accessTokenExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiresAt,
            Jti = jti
        };
    }

    /// <inheritdoc />
    public string CreateRefreshToken()
    {
        // 64 random bytes is far beyond what an attacker can guess.
        // This plain value goes to the client; only its hash is written to the database.
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        // Keyed hash (HMAC) rather than a plain hash: an attacker who steals the database still
        // cannot turn the stored values back into working tokens without RefreshSecurityKey.
        var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.RefreshSecurityKey);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToBase64String(hashBytes);
    }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.JwtSecurityKey)),

            // Lifetime is skipped on purpose: this method exists to read an already-expired token.
            // Everything else (signature, issuer, audience) is still verified.
            ValidateLifetime = false
        };

        var result = await _tokenHandler.ValidateTokenAsync(accessToken, validationParameters);

        if (!result.IsValid || result.SecurityToken is not JsonWebToken)
        {
            throw InvalidCredentialsException.WithCode();
        }

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}
