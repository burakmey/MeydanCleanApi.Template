using System.Security.Cryptography;
using System.Text;
using MeydanCleanApi.Template.Application.Abstractions.Clock;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Options.Storage;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace MeydanCleanApi.Template.Infrastructure.Services.FileStorage.Providers;

/// <summary>
/// Infrastructure local file system storage provider implementation of <see cref="IFileStorageService"/>.
/// </summary>
/// <remarks>
/// <para>
/// A cloud provider signs its own URLs and receives the bytes directly. Local storage has neither, so
/// this class does both jobs: it signs URLs that point back at this API, and it implements
/// <see cref="ILocalFileTransferService"/> for the endpoints those URLs reach. Keeping both here means
/// the signing and the verification always share one key, one clock and one path resolver.
/// </para>
/// </remarks>
public sealed class LocalStorageService : IFileStorageService, ILocalFileTransferService
{
    private readonly LocalStorageOptions _options;
    private readonly IDateTimeService _dateTimeService;

    /// <inheritdoc />
    public FileStorageType FileStorageType => FileStorageType.Local;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
    /// </summary>
    /// <param name="options">Injected local storage options model.</param>
    /// <param name="dateTimeService">Clock abstraction used for URL expiry.</param>
    public LocalStorageService(IOptions<LocalStorageOptions> options, IDateTimeService dateTimeService)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(dateTimeService);

        _options = options.Value ?? throw ConfigurationMissingException.ForSection(LocalStorageOptions.SectionName);
        _dateTimeService = dateTimeService;
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

        // Without a key every URL below would be unsigned, which means anybody who can guess a path
        // could read or overwrite stored files. Refuse to hand out a URL at all rather than a weak one.
        if (string.IsNullOrWhiteSpace(_options.SigningKey))
            throw ConfigurationMissingException.ForSection(LocalStorageOptions.SectionName, nameof(_options.SigningKey));
    }

    /// <inheritdoc />
    public Task<string> CreatePresignedUploadUrlAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Task.FromResult(BuildSignedUrl(ILocalFileTransferService.UploadOperation, path));
    }

    /// <inheritdoc />
    public Task<string> CreatePresignedDownloadUrlAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Task.FromResult(BuildSignedUrl(ILocalFileTransferService.DownloadOperation, path));
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

    /// <inheritdoc />
    public bool IsSignatureValid(string operation, string path, long expiresAtUnixSeconds, string? signature)
    {
        if (string.IsNullOrWhiteSpace(operation) || string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        EnsureConfigured();

        var now = new DateTimeOffset(DateTime.SpecifyKind(_dateTimeService.UtcNow, DateTimeKind.Utc)).ToUnixTimeSeconds();
        if (expiresAtUnixSeconds < now)
        {
            return false;
        }

        var expected = ComputeSignature(operation, path, expiresAtUnixSeconds);

        // Compare in constant time. A plain string comparison returns as soon as two bytes differ,
        // and that timing difference is enough to recover a valid signature one byte at a time.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    /// <inheritdoc />
    public async Task SaveAsync(string path, Stream content, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);

        var fullPath = ResolveFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var target = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(target, ct);
    }

    /// <inheritdoc />
    public Task<Stream?> OpenReadAsync(string path, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = ResolveFullPath(path);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    /// <summary>
    /// Builds a signed, time-limited URL pointing at this API's local transfer endpoint.
    /// </summary>
    private string BuildSignedUrl(string operation, string path)
    {
        EnsureConfigured();

        var expiresAt = new DateTimeOffset(DateTime.SpecifyKind(_dateTimeService.UtcNow, DateTimeKind.Utc))
            .AddMinutes(_options.UrlExpiryMinutes)
            .ToUnixTimeSeconds();

        var signature = ComputeSignature(operation, path, expiresAt);

        return $"{_options.BaseUrl.TrimEnd('/')}/{ILocalFileTransferService.ContentPathSegment}/{path.TrimStart('/')}" +
               $"?expires={expiresAt}&signature={signature}";
    }

    /// <summary>
    /// Produces the signature for one operation, path and expiry.
    /// </summary>
    /// <remarks>
    /// The three parts are joined with a newline so they cannot be shuffled into each other: without a
    /// separator, "upload" + "a/b" would sign the same bytes as "uploada" + "/b".
    /// </remarks>
    private string ComputeSignature(string operation, string path, long expiresAtUnixSeconds)
    {
        var payload = $"{operation}\n{path.TrimStart('/')}\n{expiresAtUnixSeconds}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        // Base64Url rather than plain Base64: the value goes in a query string, where "+" and "/"
        // would be re-interpreted by the client or the server and break the comparison.
        return WebEncoders.Base64UrlEncode(hash);
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

        // The trailing separator is what makes the prefix test correct. Comparing against the bare
        // root would also accept a sibling folder whose name merely starts with it, so a root of
        // "/srv/uploads" would let "/srv/uploads-backup/secret" through.
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(_options.RootDirectory));
        var rootWithSeparator = root + Path.DirectorySeparatorChar;
        var combined = Path.GetFullPath(Path.Combine(root, path));

        // GetFullPath has already resolved any ".." segments, so a simple prefix test is enough.
        if (!combined.StartsWith(rootWithSeparator, StringComparison.Ordinal))
        {
            throw ForbiddenException.WithCode();
        }

        return combined;
    }
}
