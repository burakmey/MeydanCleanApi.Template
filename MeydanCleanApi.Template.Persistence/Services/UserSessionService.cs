using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Clock;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Services;

/// <summary>
/// EF Core implementation of <see cref="IUserSessionService"/>.
/// </summary>
/// <remarks>
/// The session is kept on the <c>AppUsers</c> row itself, which means one active session per user:
/// signing in on a second device replaces the first. To support several devices at once, move
/// <c>RefreshTokenHash</c>, <c>RefreshTokenExpiration</c> and <c>CurrentJti</c> into their own
/// <c>UserSessions</c> table with one row per device and change the lookups below to match.
/// </remarks>
public sealed class UserSessionService(
    ApplicationDbContext context,
    IDateTimeService dateTimeService) : IUserSessionService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDateTimeService _dateTimeService = dateTimeService;

    /// <inheritdoc />
    public async Task StoreAsync(
        Guid userId, string refreshTokenHash, DateTime expiresAtUtc, string jti, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw IdNotFoundException.For<AppUser>();

        user.RefreshTokenHash = refreshTokenHash;
        user.RefreshTokenExpiration = expiresAtUtc;
        user.CurrentJti = jti;

        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Guid?> FindUserIdByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenHash)) return null;

        var now = _dateTimeService.UtcNow;

        var userId = await _context.AppUsers
            .AsNoTracking()
            .Where(u => u.RefreshTokenHash == refreshTokenHash && u.RefreshTokenExpiration > now)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(ct);

        return userId;
    }

    /// <inheritdoc />
    public async Task RevokeRefreshTokenAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenHash)) return;

        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.RefreshTokenHash == refreshTokenHash, ct);
        if (user is null) return;

        ClearSession(user);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;

        ClearSession(user);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> IsSessionActiveAsync(string refreshTokenHash, CancellationToken ct = default)
        => await FindUserIdByRefreshTokenHashAsync(refreshTokenHash, ct) is not null;

    private static void ClearSession(AppUser user)
    {
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiration = null;
        user.CurrentJti = null;
    }
}
