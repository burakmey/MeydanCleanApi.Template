namespace MeydanCleanApi.Template.Infrastructure.Options.Tokens;

/// <summary>
/// Settings for JWT access tokens and refresh tokens. Bound from the <c>Tokens:Jwt</c> section.
/// </summary>
public sealed record JwtOptions : IOptionSection
{
    /// <inheritdoc />
    public static string SectionName => "Tokens:Jwt";

    /// <summary>Minimum key length in characters. HMAC-SHA256 uses a 256-bit key, which is 32 characters.</summary>
    public const int MinimumKeyLength = 32;

    /// <summary>Gets or init-sets the secret key used for signing JWT Access Tokens.</summary>
    public required string JwtSecurityKey { get; init; }

    /// <summary>Gets or init-sets the secret key used for hashing Refresh Tokens before they are stored.</summary>
    /// <remarks>
    /// Separate from <see cref="JwtSecurityKey"/> on purpose. A leaked database holds only hashes, and
    /// turning those back into working tokens needs this key, which lives outside the database.
    /// </remarks>
    public required string RefreshSecurityKey { get; init; }

    /// <summary>Gets or init-sets the token issuer name.</summary>
    public required string Issuer { get; init; }

    /// <summary>Gets or init-sets the token audience name.</summary>
    public required string Audience { get; init; }

    /// <summary>Gets or init-sets the JWT Access Token expiration time in minutes.</summary>
    public required int AccessTokenExpiryMinutes { get; init; }

    /// <summary>Gets or init-sets the Refresh Token expiration time in days.</summary>
    public required int RefreshTokenExpirationDays { get; init; }

    /// <summary>Message shown at startup when these settings are missing or too weak.</summary>
    public const string ValidationMessage =
        "Tokens:Jwt is not configured. Set JwtSecurityKey and RefreshSecurityKey to random secrets of at " +
        "least 32 characters, plus Issuer, Audience and positive expiry values. Use user-secrets locally " +
        "and environment variables in production.";

    /// <summary>
    /// Returns whether these settings are usable. Checked once at startup by <c>ValidateOnStart</c>.
    /// </summary>
    /// <param name="options">The bound options instance.</param>
    /// <returns><c>true</c> when the application can safely sign tokens with them.</returns>
    /// <remarks>
    /// Failing here stops the app from starting. That is deliberate: a short or empty signing key
    /// produces tokens anyone can forge, and the failure would otherwise stay invisible until someone
    /// noticed forged tokens being accepted.
    /// </remarks>
    public static bool IsValid(JwtOptions options)
    {
        if (options is null) return false;

        return options.JwtSecurityKey?.Length >= MinimumKeyLength
            && options.RefreshSecurityKey?.Length >= MinimumKeyLength
            && !string.IsNullOrWhiteSpace(options.Issuer)
            && !string.IsNullOrWhiteSpace(options.Audience)
            && options.AccessTokenExpiryMinutes > 0
            && options.RefreshTokenExpirationDays > 0;
    }
}
