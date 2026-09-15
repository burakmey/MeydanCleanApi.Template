namespace MeydanCleanApi.Template.Infrastructure.Options.Storage;

/// <summary>
/// Strongly-typed configuration options model for local disk file storage.
/// </summary>
public sealed record LocalStorageOptions : IOptionSection
{
    /// <inheritdoc />
    public static string SectionName => "Storage:Local";

    /// <summary>Gets or init-sets the root directory path on disk where files are stored (configured in appsettings.json).</summary>
    public required string RootDirectory { get; init; }

    /// <summary>Gets or init-sets the public base URL path for retrieving stored files (configured in appsettings.json).</summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Gets or init-sets the secret used to sign local upload and download URLs.
    /// </summary>
    /// <remarks>
    /// Local storage has no cloud provider to sign URLs for it, so the API signs them itself.
    /// Without this key anybody who can guess a path could read or overwrite stored files, so
    /// treat it like any other secret: keep it out of the repository and supply it per environment.
    /// </remarks>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets or init-sets how many minutes a signed local URL stays usable. Defaults to 15.
    /// </summary>
    public int UrlExpiryMinutes { get; init; } = 15;
}
