using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Abstractions.Storage;

/// <summary>
/// Service contract for direct-to-cloud storage providers (AWS S3, Azure Blob, Supabase Storage, Local Storage).
/// </summary>
/// <remarks>
/// Direct-to-cloud upload workflow: Bytes do not pass through the API web server.
/// The client requests a presigned upload URL from the API and uploads directly to the cloud provider.
/// </remarks>
public interface IFileStorageService
{
    /// <summary>
    /// Gets the target cloud storage provider type handled by this implementation.
    /// </summary>
    FileStorageType FileStorageType { get; }

    /// <summary>
    /// Generates a time-limited presigned URL allowing the client to HTTP PUT upload a file directly to cloud storage.
    /// </summary>
    /// <param name="path">Cloud object key or path (e.g., "products/images/sample.jpg").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Presigned upload URL string.</returns>
    Task<string> CreatePresignedUploadUrlAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Generates a time-limited presigned URL allowing the client to GET read/download a private cloud file.
    /// </summary>
    /// <param name="path">Cloud object key or path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Presigned download URL string.</returns>
    Task<string> CreatePresignedDownloadUrlAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Checks whether an object exists at the specified cloud storage path.
    /// </summary>
    /// <param name="path">Cloud object key or path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the object exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Deletes an object from cloud storage at the specified path.
    /// </summary>
    /// <param name="path">Cloud object key or path.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(string path, CancellationToken ct = default);
}
