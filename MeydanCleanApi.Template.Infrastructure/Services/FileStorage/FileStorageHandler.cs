namespace MeydanCleanApi.Template.Infrastructure.Services.FileStorage;

/// <summary>
/// Infrastructure orchestrator implementing <see cref="IFileStorageHandler"/> by delegating storage calls to the resolved <see cref="IFileStorageService"/>.
/// </summary>
public sealed class FileStorageHandler(IFileStorageHandlerResolver resolver) : IFileStorageHandler
{
    /// <inheritdoc />
    public async Task<string> CreatePresignedUploadUrlAsync(FileStorageType storageType, string path, CancellationToken ct = default)
    {
        var service = resolver.Resolve(storageType);
        return await service.CreatePresignedUploadUrlAsync(path, ct);
    }

    /// <inheritdoc />
    public async Task<string> CreatePresignedDownloadUrlAsync(FileStorageType storageType, string path, CancellationToken ct = default)
    {
        var service = resolver.Resolve(storageType);
        return await service.CreatePresignedDownloadUrlAsync(path, ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(FileStorageType storageType, string path, CancellationToken ct = default)
    {
        var service = resolver.Resolve(storageType);
        await service.DeleteAsync(path, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(FileStorageType storageType, string path, CancellationToken ct = default)
    {
        var service = resolver.Resolve(storageType);
        return await service.ExistsAsync(path, ct);
    }
}
