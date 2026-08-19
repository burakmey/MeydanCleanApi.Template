using MeydanCleanApi.Template.Application.Common.Models.Token;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Domain-level contract managing user login, refresh tokens, and external OAuth authentications.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password, issuing access and refresh tokens.
    /// </summary>
    Task<TokenModel> LoginAsync(string email, string password, CancellationToken ct = default);

    /// <summary>
    /// Renews an access token using a valid refresh token.
    /// </summary>
    Task<TokenModel> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Authenticates or registers a user via an external OAuth provider (e.g. Google).
    /// </summary>
    Task<TokenModel> ExternalLoginAsync(AuthProviderType authProvider, string idToken, CancellationToken ct = default);
}
