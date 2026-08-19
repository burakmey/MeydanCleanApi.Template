namespace MeydanCleanApi.Template.Domain.Entities.Files;

/// <summary>
/// Represents stored file metadata for direct-to-cloud file uploads.
/// </summary>
/// <remarks>
/// This describes the stored object and nothing else: where it lives, how big it is, what type it is.
/// It deliberately says nothing about who owns it or what it is for. Those are properties of the
/// <em>relationship</em>, so they live on the attachment row (see <see cref="Base.BaseFileAttachment"/>).
/// Keeping the two apart is what lets one file type serve every feature without growing extra columns.
/// </remarks>
public class FileEntity : GuidEntity
{
    /// <summary>
    /// Gets or sets the foreign key identifier of the target storage provider.
    /// </summary>
    public required int FileStorageId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the file upload status.
    /// </summary>
    public required int FileStatusId { get; set; }

    /// <summary>
    /// Gets or sets the original file name provided by the client.
    /// </summary>
    public required string OriginalFileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type of the file (e.g., "image/jpeg").
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    public required long SizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets the storage path or key for the file in cloud storage.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Gets the filename component extracted from <see cref="Path"/>.
    /// </summary>
    public string StoredFileName => System.IO.Path.GetFileName(Path);

    /// <summary>
    /// Gets the extension component (including dot) extracted from <see cref="Path"/>.
    /// </summary>
    public string Extension => System.IO.Path.GetExtension(Path);

    /// <summary>
    /// Gets or sets the associated storage provider navigation property.
    /// </summary>
    public FileStorage? FileStorage { get; set; }

    /// <summary>
    /// Gets or sets the associated upload status navigation property.
    /// </summary>
    public FileStatus? FileStatus { get; set; }
}
