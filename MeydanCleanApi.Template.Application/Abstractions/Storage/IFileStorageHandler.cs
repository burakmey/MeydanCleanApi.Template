using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Abstractions.Storage;

/// <summary>
/// Dispatches file storage operations to the registered provider matching <see cref="FileStorageType"/>.
/// </summary>
public interface IFileStorageHandler
{
    /// <summary>
    /// Generates a presigned URL allowing direct client HTTP PUT upload to the specified storage provider.
    /// </summary>
    Task<string> CreatePresignedUploadUrlAsync(FileStorageType storageType, string path, CancellationToken ct = default);

    /// <summary>
    /// Generates a presigned URL allowing client GET read access from the specified storage provider.
    /// </summary>
    Task<string> CreatePresignedDownloadUrlAsync(FileStorageType storageType, string path, CancellationToken ct = default);

    /// <summary>
    /// Permanently removes the object at <paramref name="path"/> from the specified storage provider.
    /// </summary>
    Task DeleteAsync(FileStorageType storageType, string path, CancellationToken ct = default);

    /// <summary>
    /// Checks whether an object exists at <paramref name="path"/> on the specified storage provider.
    /// </summary>
    Task<bool> ExistsAsync(FileStorageType storageType, string path, CancellationToken ct = default);
}
