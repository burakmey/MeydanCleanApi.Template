using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Options.Storage;
using Microsoft.Extensions.Options;
using Supabase.Storage;
using Supabase.Storage.Interfaces;
using Client = Supabase.Client;

namespace MeydanCleanApi.Template.Infrastructure.Services.FileStorage.Providers;

/// <summary>
/// Infrastructure Supabase cloud object storage provider implementation of <see cref="IFileStorageService"/>.
/// Connects to Supabase Storage SDK for generating presigned upload/download URLs and object management.
/// </summary>
public sealed class SupabaseStorageService : IFileStorageService
{
    private readonly SupabaseOptions _options;
    private IStorageFileApi<FileObject>? _bucket;
    private readonly SemaphoreSlim _initLock = new(initialCount: 1, maxCount: 1);

    /// <inheritdoc />
    public FileStorageType FileStorageType => FileStorageType.Supabase;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupabaseStorageService"/> class.
    /// </summary>
    /// <param name="options">Injected Supabase storage options model.</param>
    public SupabaseStorageService(IOptions<SupabaseOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? throw ConfigurationMissingException.ForSection(SupabaseOptions.SectionName);
    }

    /// <inheritdoc />
    public async Task<string> CreatePresignedUploadUrlAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var bucket = await GetBucketAsync(ct);

        var uploadSignedUrl = await bucket.CreateUploadSignedUrl(path);
        return uploadSignedUrl.SignedUrl.ToString();
    }

    /// <inheritdoc />
    public async Task<string> CreatePresignedDownloadUrlAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var bucket = await GetBucketAsync(ct);

        var expiryInSeconds = _options.DownloadUrlExpiryMinute * 60;
        var signedUrl = await bucket.CreateSignedUrl(path, expiryInSeconds);

        return signedUrl;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var bucket = await GetBucketAsync(ct);

        var lastSlash = path.LastIndexOf('/');
        var folder = lastSlash >= 0 ? path[..lastSlash] : string.Empty;
        var fileName = lastSlash >= 0 ? path[(lastSlash + 1)..] : path;

        var matches = await bucket.List(folder, new SearchOptions { Search = fileName });

        return matches?.Any(item => item.Name == fileName) == true;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var bucket = await GetBucketAsync(ct);

        await bucket.Remove([path]);
    }

    private async Task<IStorageFileApi<FileObject>> GetBucketAsync(CancellationToken ct)
    {
        if (!_options.IsConfigured)
        {
            throw ConfigurationMissingException.ForSection(SupabaseOptions.SectionName, nameof(_options.Url));
        }

        if (_bucket is not null) return _bucket;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_bucket is not null) return _bucket;

            var client = new Client(_options.Url, _options.ApiKey, new Supabase.SupabaseOptions { AutoConnectRealtime = false });
            await client.InitializeAsync();

            _bucket = client.Storage.From(_options.Bucket);
            return _bucket;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
