using System.Security.Claims;
using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Infrastructure.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace MeydanCleanApi.Template.Infrastructure.Services.User;

/// <summary>
/// Infrastructure service providing type-safe access to the authenticated user identity via <see cref="IHttpContextAccessor"/>.
/// </summary>
/// <remarks>
/// Claims are read using the short names the token is written with ("sub", "email", "role"), because
/// <c>MapInboundClaims</c> is switched off in the JWT bearer setup. The long <c>ClaimTypes.*</c> URIs are
/// checked as a fallback so this still works if you ever turn claim mapping back on.
/// </remarks>
/// <param name="httpContextAccessor">HTTP context accessor service.</param>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var value = FindFirstValue(JwtClaimTypes.Subject, ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var parsedGuid) ? parsedGuid : null;
        }
    }

    /// <inheritdoc />
    public string? Email => FindFirstValue(JwtClaimTypes.Email, ClaimTypes.Email);

    /// <inheritdoc />
    public IReadOnlyList<string> Roles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return [];

            return [.. user.FindAll(JwtClaimTypes.Role)
                .Concat(user.FindAll(ClaimTypes.Role))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)];
        }
    }

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private string? FindFirstValue(string primaryClaimType, string fallbackClaimType)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null;

        return user.FindFirst(primaryClaimType)?.Value ?? user.FindFirst(fallbackClaimType)?.Value;
    }
}
