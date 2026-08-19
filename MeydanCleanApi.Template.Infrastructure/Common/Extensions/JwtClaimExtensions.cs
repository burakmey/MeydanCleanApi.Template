using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MeydanCleanApi.Template.Infrastructure.Common.Extensions;

/// <summary>
/// Extension methods for safely extracting claims from <see cref="JsonWebToken"/>.
/// </summary>
public static class JwtClaimExtensions
{
    /// <summary>
    /// Gets a required claim string value or throws <see cref="SecurityTokenException"/> if missing.
    /// </summary>
    public static string GetRequiredClaimValue(this JsonWebToken token, string claimType)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(claimType);

        var value = token.GetClaimValue(claimType);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new SecurityTokenException($"Required claim '{claimType}' is missing from token.");
        }

        return value;
    }

    /// <summary>
    /// Gets an optional claim string value or returns null if not present.
    /// </summary>
    public static string? GetClaimValue(this JsonWebToken token, string claimType)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(claimType);

        return token.Claims.FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
