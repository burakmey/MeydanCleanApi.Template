using System.Text.RegularExpressions;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Common.Models.FileStorage;
using MeydanCleanApi.Template.Application.Constants.FileStorage;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Files;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Services.FileStorage;

/// <summary>
/// Application-layer facade orchestrating file metadata persistence (<see cref="FileEntity"/>) and storage provider operations.
/// Dynamically resolves the target storage provider via <see cref="IFileStorageHandler"/> based on <see cref="FileEntity.FileStorageId"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileStorageCoordinator"/> class.
/// </remarks>
public sealed partial class FileStorageCoordinator(
    IWriteRepository<FileEntity, Guid> fileWriteRepository,
    IFileStorageHandler storageHandler) : IFileStorageCoordinator
{
    private const FileStorageType DefaultStorageType = FileStorageType.Local;

    private readonly IWriteRepository<FileEntity, Guid> _fileWriteRepository = fileWriteRepository;
    private readonly IFileStorageHandler _storageHandler = storageHandler;

    /// <summary>
    /// Sub-folder names may only contain letters, digits, dash and underscore.
    /// This rules out "..", slashes and drive letters, which is what makes path traversal possible.
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z0-9_-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeSubFolderPattern();

    /// <inheritdoc />
    public async Task<FileEntity> CreatePendingAsync(
        FileUploadModel file, string container, string? subFolder = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        // 1. The container and sub-folder come from the caller, so both are checked before they are
        //    used to build a path. Anything not on the whitelist is rejected outright.
        if (!FileStorageContainers.IsAllowed(container))
        {
            throw ValidationException.From(new Dictionary<string, string[]>
            {
                [nameof(container)] = [ValidationCodes.FileContainerInvalid]
            });
        }

        if (!string.IsNullOrWhiteSpace(subFolder) && !SafeSubFolderPattern().IsMatch(subFolder))
        {
            throw ValidationException.From(new Dictionary<string, string[]>
            {
                [nameof(subFolder)] = [ValidationCodes.FileSubFolderInvalid]
            });
        }

        // 2. Build the stored name from a new id plus the original extension. The client's file name
        //    is kept only as metadata, never as part of the path, so a name like "../../x.txt"
        //    cannot influence where the object lands.
        var fileId = Guid.NewGuid();
        var extension = SanitizeExtension(file.OriginalFileName);

        var storagePath = string.IsNullOrWhiteSpace(subFolder)
            ? $"{container}/{fileId}{extension}"
            : $"{container}/{subFolder}/{fileId}{extension}";

        // 3. Stage the metadata row. Status stays Pending until the upload is confirmed.
        var fileEntity = new FileEntity
        {
            Id = fileId,
            FileStorageId = (int)DefaultStorageType,
            FileStatusId = (int)FileStatusType.Pending,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            SizeInBytes = file.SizeInBytes,
            Path = storagePath
        };

        await _fileWriteRepository.AddAsync(fileEntity, ct);

        return fileEntity;
    }

    /// <inheritdoc />
    public async Task<string> CreatePresignedUploadUrlAsync(FileEntity fileEntity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileEntity);
        var storageType = (FileStorageType)fileEntity.FileStorageId;
        return await _storageHandler.CreatePresignedUploadUrlAsync(storageType, fileEntity.Path, ct);
    }

    /// <inheritdoc />
    public async Task<string> CreatePresignedDownloadUrlAsync(FileEntity fileEntity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileEntity);
        var storageType = (FileStorageType)fileEntity.FileStorageId;
        return await _storageHandler.CreatePresignedDownloadUrlAsync(storageType, fileEntity.Path, ct);
    }

    /// <inheritdoc />
    public async Task DeleteFromStorageAsync(FileEntity fileEntity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileEntity);
        var storageType = (FileStorageType)fileEntity.FileStorageId;
        await _storageHandler.DeleteAsync(storageType, fileEntity.Path, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmUploadedAsync(FileEntity fileEntity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileEntity);

        // Confirming twice is harmless: the second call sees the status is already Uploaded and stops.
        if (fileEntity.FileStatusId == (int)FileStatusType.Uploaded)
        {
            return true;
        }

        var storageType = (FileStorageType)fileEntity.FileStorageId;
        var exists = await _storageHandler.ExistsAsync(storageType, fileEntity.Path, ct);
        if (exists)
        {
            fileEntity.FileStatusId = (int)FileStatusType.Uploaded;
        }

        return exists;
    }

    /// <summary>
    /// Returns a safe lower-case extension, or an empty string when the name has none.
    /// </summary>
    private static string SanitizeExtension(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;

        // Keep only a dot followed by letters and digits. Anything else is dropped rather than trusted.
        extension = extension.ToLowerInvariant();
        return extension.Length <= 16 && extension[1..].All(char.IsLetterOrDigit)
            ? extension
            : string.Empty;
    }
}
