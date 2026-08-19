using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Options.Storage;
using Microsoft.Extensions.Options;

namespace MeydanCleanApi.Template.Infrastructure.Services.FileStorage.Providers;

/// <summary>
/// Infrastructure local file system storage provider implementation of <see cref="IFileStorageService"/>.
/// </summary>
public sealed class LocalStorageService : IFileStorageService
{
    private readonly LocalStorageOptions _options;

    /// <inheritdoc />
    public FileStorageType FileStorageType => FileStorageType.Local;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
    /// </summary>
    /// <param name="options">Injected local storage options model.</param>
    public LocalStorageService(IOptions<LocalStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? throw ConfigurationMissingException.ForSection(LocalStorageOptions.SectionName);
    }

    /// <summary>
    /// Throws a clear configuration error if local storage has not been set up.
    /// </summary>
    /// <remarks>
    /// Checked on use rather than in the constructor. Every storage provider is constructed when the
    /// resolver is built, so a provider this application never uses must not stop the app from starting.
    /// </remarks>
    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.RootDirectory))
            throw ConfigurationMissingException.ForSection(LocalStorageOptions.SectionName, nameof(_options.RootDirectory));

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            throw ConfigurationMissingException.ForSection(LocalStorageOptions.SectionName, nameof(_options.BaseUrl));
    }

    /// <inheritdoc />
    public Task<string> CreatePresignedUploadUrlAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureConfigured();

        var url = $"{_options.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public Task<string> CreatePresignedDownloadUrlAsync(string path, CancellationToken ct = default)
    {
        return CreatePresignedUploadUrlAsync(path, ct);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = ResolveFullPath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Task.FromResult(File.Exists(ResolveFullPath(path)));
    }

    /// <summary>
    /// Turns a relative storage path into a full disk path and refuses anything outside the root folder.
    /// </summary>
    /// <param name="path">Relative object path, for example <c>public/products/{id}.jpg</c>.</param>
    /// <returns>The absolute path on disk.</returns>
    /// <exception cref="ForbiddenException">Thrown when the path would escape the configured root.</exception>
    /// <remarks>
    /// This is the last line of defence against path traversal. Callers already validate the container
    /// and sub-folder, but the check is repeated here because this class is the thing that actually
    /// touches the file system. Note that <c>Path.Combine</c> throws away the first argument when the
    /// second one is rooted, so "C:/Windows/x" would otherwise ignore the storage folder completely.
    /// </remarks>
    private string ResolveFullPath(string path)
    {
        EnsureConfigured();

        var root = Path.GetFullPath(_options.RootDirectory);
        var combined = Path.GetFullPath(Path.Combine(root, path));

        // GetFullPath has already resolved any ".." segments, so a simple prefix test is enough.
        if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw ForbiddenException.WithCode();
        }

        return combined;
    }
}
