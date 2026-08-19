namespace MeydanCleanApi.Template.Application.Common.Models.FileStorage;

/// <summary>
/// User-supplied file metadata (original file name, MIME content type, size in bytes) for direct-to-cloud upload requests.
/// </summary>
public sealed record FileUploadModel
{
    /// <summary>
    /// Gets or sets the original file name provided by the client (including extension).
    /// </summary>
    public required string OriginalFileName { get; init; }

    /// <summary>
    /// Gets or sets the MIME content type declared by the client (e.g., "image/webp").
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets or sets the declared file size in bytes.
    /// </summary>
    public required long SizeInBytes { get; init; }

    /// <summary>
    /// Gets the lower-cased file extension (including leading dot).
    /// </summary>
    public string Extension => System.IO.Path.GetExtension(OriginalFileName).ToLowerInvariant();
}
