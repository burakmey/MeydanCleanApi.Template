namespace MeydanCleanApi.Template.Application.Common.Models.Token;

/// <summary>
/// Contains JWT Access Token, Refresh Token, expiration metadata, and JTI (JWT ID) claims.
/// </summary>
public record TokenModel
{
    /// <summary>
    /// Gets or sets the signed JWT Access Token string.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Gets or sets the Access Token expiration moment in UTC.
    /// </summary>
    public required DateTime AccessTokenExpiration { get; init; }

    /// <summary>
    /// Gets or sets the plain-text Refresh Token string (only its hash is persisted in DB).
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Gets or sets the Refresh Token expiration moment in UTC.
    /// </summary>
    public required DateTime RefreshTokenExpiration { get; init; }

    /// <summary>
    /// Gets or sets the unique JWT identifier (<c>jti</c> claim).
    /// </summary>
    public required string Jti { get; init; }
}
