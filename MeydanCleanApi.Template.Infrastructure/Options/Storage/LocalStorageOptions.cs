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
}
