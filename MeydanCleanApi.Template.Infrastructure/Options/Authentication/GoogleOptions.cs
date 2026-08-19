namespace MeydanCleanApi.Template.Infrastructure.Options.Authentication;

/// <summary>
/// Strongly-typed configuration options model for Google external OAuth2 authentication provider.
/// </summary>
public sealed record GoogleOptions : IOptionSection
{
    /// <inheritdoc />
    public static string SectionName => "Authentication:Google";

    /// <summary>Gets or init-sets the Google Client ID (e.g. "12345-abc.apps.googleusercontent.com").</summary>
    public required string ClientId { get; init; }

    /// <summary>Gets or init-sets the expected token issuer URL (typically "https://accounts.google.com").</summary>
    public required string Issuer { get; init; }

    /// <summary>Gets or init-sets the discovery metadata document URL (typically "https://accounts.google.com/.well-known/openid-configuration").</summary>
    public required string MetaData { get; init; }
}
