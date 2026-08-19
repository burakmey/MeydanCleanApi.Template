namespace MeydanCleanApi.Template.Application.Common.Models.FileStorage;

/// <summary>
/// DTO representing a file entity's metadata for admin and application queries.
/// </summary>
/// <remarks>
/// The internal storage path is deliberately not exposed. Clients get a time-limited download URL
/// instead, so they never learn how objects are laid out inside the bucket.
/// </remarks>
/// <param name="Id">File identifier.</param>
/// <param name="FileStorageId">Storage provider the object lives on.</param>
/// <param name="FileStatusId">Upload status (Pending, Uploaded, Failed).</param>
/// <param name="OriginalFileName">Name the client uploaded the file under.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="SizeInBytes">Declared size in bytes.</param>
/// <param name="CreatedAt">Creation timestamp in UTC.</param>
public sealed record FileDto(
    Guid Id,
    int FileStorageId,
    int FileStatusId,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    DateTime CreatedAt
);
