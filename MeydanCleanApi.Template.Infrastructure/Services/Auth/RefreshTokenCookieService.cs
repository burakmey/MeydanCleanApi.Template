using MeydanCleanApi.Template.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Http;

namespace MeydanCleanApi.Template.Infrastructure.Services.Auth;

/// <summary>
/// Delivers the refresh token to the browser in an HttpOnly cookie.
/// </summary>
/// <remarks>
/// The cookie flags matter:
/// <list type="bullet">
/// <item><c>HttpOnly</c> keeps page scripts from reading the token, so a cross-site scripting bug cannot steal it.</item>
/// <item><c>Secure</c> means it is only sent over HTTPS.</item>
/// <item><c>SameSite=Strict</c> means the browser will not attach it to requests started by another site, which blocks CSRF.</item>
/// <item><c>Path</c> limits the cookie to the auth endpoints, so it is not sent with every API call.</item>
/// </list>
/// Native mobile clients that cannot hold cookies should implement this interface differently,
/// for example by returning the token in a response header the app stores in its keychain.
/// </remarks>
public sealed class RefreshTokenCookieService(IHttpContextAccessor httpContextAccessor) : IRefreshTokenDeliveryService
{
    /// <summary>Name of the cookie carrying the refresh token.</summary>
    public const string CookieName = "refreshToken";

    /// <summary>Path the cookie is scoped to.</summary>
    public const string CookiePath = "/api/v1/auth";

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public void Issue(string refreshToken, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var response = _httpContextAccessor.HttpContext?.Response;
        if (response is null) return;

        response.Cookies.Append(CookieName, refreshToken, BuildOptions(expiresAtUtc));
    }

    /// <inheritdoc />
    public string? Read()
    {
        var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
        if (cookies is null) return null;

        return cookies.TryGetValue(CookieName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    /// <inheritdoc />
    public void Revoke()
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response is null) return;

        // Deleting has to repeat the same flags the cookie was written with,
        // otherwise the browser treats it as a different cookie and keeps the original.
        response.Cookies.Delete(CookieName, BuildOptions(DateTime.UnixEpoch));
    }

    private static CookieOptions BuildOptions(DateTime expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expiresAtUtc
    };
}
