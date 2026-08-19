using MeydanCleanApi.Template.Application.Common.Models.FileStorage;
using MeydanCleanApi.Template.Domain.Entities.Files;

namespace MeydanCleanApi.Template.Application.Abstractions.Storage;

/// <summary>
/// Application-layer orchestrator for file upload workflows, DB staging (<see cref="FileEntity"/>), and storage provider dispatching.
/// </summary>
public interface IFileStorageCoordinator
{
    /// <summary>
    /// Creates a new <see cref="FileEntity"/> metadata record in Pending status and stages it for persistence.
    /// </summary>
    /// <param name="file">Client-supplied file metadata (name, content type, size).</param>
    /// <param name="container">Container root. Must be one of <c>FileStorageContainers.All</c>.</param>
    /// <param name="subFolder">
    /// Optional grouping folder, for example a parent entity id. This is for <b>server-side callers
    /// only</b> — never pass a value that came from the client, or a caller could steer the object
    /// outside its container.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<FileEntity> CreatePendingAsync(FileUploadModel file, string container, string? subFolder = null, CancellationToken ct = default);

    /// <summary>
    /// Generates a presigned upload URL for the specified file entity.
    /// </summary>
    Task<string> CreatePresignedUploadUrlAsync(FileEntity fileEntity, CancellationToken ct = default);

    /// <summary>
    /// Generates a presigned download URL for the specified file entity.
    /// </summary>
    Task<string> CreatePresignedDownloadUrlAsync(FileEntity fileEntity, CancellationToken ct = default);

    /// <summary>
    /// Permanently removes the file from cloud storage.
    /// </summary>
    Task DeleteFromStorageAsync(FileEntity fileEntity, CancellationToken ct = default);

    /// <summary>
    /// Verifies that uploaded bytes reached cloud storage and updates the entity status to Uploaded.
    /// </summary>
    Task<bool> ConfirmUploadedAsync(FileEntity fileEntity, CancellationToken ct = default);
}
