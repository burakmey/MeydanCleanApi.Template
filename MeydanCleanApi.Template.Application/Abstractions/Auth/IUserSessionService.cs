namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Stores and revokes user refresh token sessions.
/// </summary>
/// <remarks>
/// Refresh tokens are never stored in plain text. Callers hash the token first
/// (see <see cref="ITokenService.HashRefreshToken"/>) and pass the hash to every method here.
/// </remarks>
public interface IUserSessionService
{
    /// <summary>
    /// Saves the session for a user, replacing any session that was already stored.
    /// </summary>
    /// <param name="userId">Owner of the session.</param>
    /// <param name="refreshTokenHash">Hash of the issued refresh token.</param>
    /// <param name="expiresAtUtc">When the refresh token stops being valid.</param>
    /// <param name="jti">Identifier of the access token issued alongside it.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreAsync(Guid userId, string refreshTokenHash, DateTime expiresAtUtc, string jti, CancellationToken ct = default);

    /// <summary>
    /// Returns the id of the user holding an unexpired session for this token hash, or <c>null</c>.
    /// </summary>
    /// <param name="refreshTokenHash">Hash of the refresh token presented by the client.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Guid?> FindUserIdByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);

    /// <summary>
    /// Revokes the session matching a refresh token hash.
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshTokenHash, CancellationToken ct = default);

    /// <summary>
    /// Revokes every active session belonging to a user.
    /// </summary>
    Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a refresh token session exists and has not expired.
    /// </summary>
    Task<bool> IsSessionActiveAsync(string refreshTokenHash, CancellationToken ct = default);
}
