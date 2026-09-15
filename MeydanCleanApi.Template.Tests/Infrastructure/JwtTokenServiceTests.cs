using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Common.Constants;
using MeydanCleanApi.Template.Infrastructure.Options.Tokens;
using MeydanCleanApi.Template.Infrastructure.Services.Auth.Tokens;
using MeydanCleanApi.Template.Tests.Common;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeydanCleanApi.Template.Tests.Infrastructure;

/// <summary>
/// Covers token issuing and the keyed hash that keeps plain refresh tokens out of the database.
/// </summary>
public sealed class JwtTokenServiceTests
{
    private const string JwtKey = "unit-test-jwt-signing-key-at-least-32-chars";
    private const string RefreshKey = "unit-test-refresh-hmac-key-at-least-32-chars";

    private static readonly Guid UserId = new("2f1c9a44-0000-0000-0000-00000000abcd");
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FixedClock _clock = new(Now);

    private JwtTokenService CreateService(string refreshKey = RefreshKey)
    {
        var options = Options.Create(new JwtOptions
        {
            JwtSecurityKey = JwtKey,
            RefreshSecurityKey = refreshKey,
            Issuer = "MeydanCleanApi.Tests",
            Audience = "MeydanCleanApi.Tests.Clients",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

        return new JwtTokenService(options, _clock);
    }

    [Fact]
    public void ExpiryIsTakenFromTheClock_NotFromTheSystemTime()
    {
        var tokens = CreateService().CreateTokens(UserId, "user@example.com", new[] { "Customer" });

        Assert.Equal(Now.AddMinutes(15), tokens.AccessTokenExpiration);
        Assert.Equal(Now.AddDays(7), tokens.RefreshTokenExpiration);
    }

    [Fact]
    public void EachIssuedPair_GetsItsOwnJtiAndRefreshToken()
    {
        var service = CreateService();

        var first = service.CreateTokens(UserId, "user@example.com", new[] { "Customer" });
        var second = service.CreateTokens(UserId, "user@example.com", new[] { "Customer" });

        Assert.NotEqual(first.Jti, second.Jti);
        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
    }

    [Fact]
    public async Task AnIssuedTokenCarriesTheClaimsTheApiAuthorizesOn()
    {
        var service = CreateService();
        var tokens = service.CreateTokens(UserId, "user@example.com", new[] { "Customer", "Admin" });

        var principal = await service.GetPrincipalFromExpiredTokenAsync(tokens.AccessToken);

        Assert.Equal(UserId.ToString(), principal.FindFirst(JwtClaimTypes.Subject)?.Value);
        Assert.Equal("user@example.com", principal.FindFirst(JwtClaimTypes.Email)?.Value);
        Assert.Equal(tokens.Jti, principal.FindFirst(JwtClaimTypes.Jti)?.Value);

        var roles = principal.FindAll(JwtClaimTypes.Role).Select(claim => claim.Value).ToArray();
        Assert.Equal(2, roles.Length);
        Assert.Contains("Customer", roles);
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public async Task ATokenSignedWithAnotherKey_IsRefused()
    {
        var foreignService = new JwtTokenService(
            Options.Create(new JwtOptions
            {
                JwtSecurityKey = "a-completely-different-signing-key-32-chars",
                RefreshSecurityKey = RefreshKey,
                Issuer = "MeydanCleanApi.Tests",
                Audience = "MeydanCleanApi.Tests.Clients",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpirationDays = 7
            }),
            _clock);

        var foreignToken = foreignService.CreateTokens(UserId, "user@example.com", Array.Empty<string>()).AccessToken;

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => CreateService().GetPrincipalFromExpiredTokenAsync(foreignToken));
    }

    [Fact]
    public void HashingTheSameRefreshToken_AlwaysGivesTheSameValue()
    {
        // The stored hash is looked up on every refresh, so it has to be reproducible.
        var service = CreateService();

        Assert.Equal(service.HashRefreshToken("a-refresh-token"), service.HashRefreshToken("a-refresh-token"));
    }

    [Fact]
    public void DifferentRefreshTokens_HashDifferently()
    {
        var service = CreateService();

        Assert.NotEqual(service.HashRefreshToken("one"), service.HashRefreshToken("two"));
    }

    [Fact]
    public void TheHashIsKeyed_SoADatabaseLeakAloneCannotReproduceIt()
    {
        // This is why RefreshSecurityKey exists: the same token under a different key hashes
        // differently, so stolen rows cannot be turned back into working tokens.
        var token = "a-refresh-token";

        Assert.NotEqual(
            CreateService().HashRefreshToken(token),
            CreateService(refreshKey: "a-different-refresh-hmac-key-32-chars").HashRefreshToken(token));
    }

    [Fact]
    public void ThePlainRefreshTokenIsNeverItsOwnHash()
    {
        var service = CreateService();
        var token = service.CreateRefreshToken();

        Assert.NotEmpty(token);
        Assert.NotEqual(token, service.HashRefreshToken(token));
    }
}
