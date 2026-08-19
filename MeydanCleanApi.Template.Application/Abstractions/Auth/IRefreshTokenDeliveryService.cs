namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Hands a refresh token to the client and reads it back on the next request.
/// </summary>
/// <remarks>
/// Handlers call this instead of returning the refresh token in the response body, so the secret
/// never appears in JSON. How the token actually travels is an infrastructure decision: the shipped
/// implementation uses an HttpOnly cookie, which page scripts cannot read.
/// </remarks>
public interface IRefreshTokenDeliveryService
{
    /// <summary>
    /// Sends a newly issued refresh token to the client.
    /// </summary>
    /// <param name="refreshToken">The plain refresh token.</param>
    /// <param name="expiresAtUtc">When the token stops being valid.</param>
    void Issue(string refreshToken, DateTime expiresAtUtc);

    /// <summary>
    /// Reads the refresh token the client sent with the current request, or <c>null</c> when absent.
    /// </summary>
    string? Read();

    /// <summary>
    /// Removes the refresh token from the client.
    /// </summary>
    void Revoke();
}
