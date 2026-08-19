using System.Security.Claims;
using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Service contract for generating access tokens, refresh tokens, and extracting claim principals from expired tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a pair of access token and refresh token for the specified user claims.
    /// </summary>
    /// <param name="userId">Target user identifier.</param>
    /// <param name="email">Target user email.</param>
    /// <param name="roles">Target user roles.</param>
    /// <returns>A new <see cref="TokenModel"/> containing access token, refresh token, and expiration timestamp.</returns>
    TokenModel CreateTokens(Guid userId, string email, IEnumerable<string> roles);

    /// <summary>
    /// Generates a cryptographically secure random refresh token string.
    /// </summary>
    /// <returns>Base64-encoded random refresh token string. This is the value handed to the client.</returns>
    string CreateRefreshToken();

    /// <summary>
    /// Hashes a refresh token for database storage.
    /// </summary>
    /// <param name="refreshToken">The plain refresh token issued to the client.</param>
    /// <returns>A stable hash of the token.</returns>
    /// <remarks>
    /// Only the hash is stored. If the database leaks, the stored values cannot be replayed as tokens
    /// because producing a valid token also requires the server-side signing key.
    /// The same input always produces the same output, so lookups by hash work.
    /// </remarks>
    string HashRefreshToken(string refreshToken);

    /// <summary>
    /// Extracts <see cref="ClaimsPrincipal"/> from an expired access token without validating token lifetime.
    /// </summary>
    /// <param name="accessToken">The expired access token string.</param>
    /// <returns>Extracted claims principal.</returns>
    Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string accessToken);
}
