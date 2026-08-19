using MeydanCleanApi.Template.Application.Common.Models.Auth;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Application security contract for verifying external OAuth2 ID tokens (e.g. Google, Apple, Microsoft).
/// </summary>
public interface IExternalAuthVerifier
{
    /// <summary>
    /// Gets the authentication provider type associated with this verifier instance.
    /// </summary>
    AuthProviderType AuthProvider { get; }

    /// <summary>
    /// Verifies the external ID token and extracts verified user details.
    /// </summary>
    /// <param name="idToken">The raw ID token string provided by client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A verified <see cref="ExternalUserModel"/> instance.</returns>
    Task<ExternalUserModel> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}
