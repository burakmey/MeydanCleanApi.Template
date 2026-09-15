using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Options.Storage;
using MeydanCleanApi.Template.Infrastructure.Services.FileStorage.Providers;
using MeydanCleanApi.Template.Tests.Common;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeydanCleanApi.Template.Tests.Infrastructure;

/// <summary>
/// Covers the signed URLs local storage issues for itself, and the traversal guard behind them.
/// </summary>
public sealed class LocalStorageServiceTests : IDisposable
{
    private const string SigningKey = "unit-test-signing-key-at-least-32-characters";
    private const string BaseUrl = "/api/v1/files";
    private const string StoredPath = "temp/8f6d1b3e-0000-0000-0000-000000000001.txt";

    private readonly string _root;
    private readonly FixedClock _clock = new(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

    public LocalStorageServiceTests()
    {
        // A directory per test class instance, so the round-trip test writes somewhere real
        // without colliding with anything else.
        _root = Path.Combine(Path.GetTempPath(), "meydan-local-storage-tests", Guid.NewGuid().ToString("N"), "uploads");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        var parent = Directory.GetParent(_root)?.FullName;
        if (parent is not null && Directory.Exists(parent))
        {
            Directory.Delete(parent, recursive: true);
        }
    }

    private LocalStorageService CreateService(string? signingKey = SigningKey, int expiryMinutes = 15)
    {
        var options = Options.Create(new LocalStorageOptions
        {
            RootDirectory = _root,
            BaseUrl = BaseUrl,
            SigningKey = signingKey ?? string.Empty,
            UrlExpiryMinutes = expiryMinutes
        });

        return new LocalStorageService(options, _clock);
    }

    private static (string Path, long Expires, string Signature) ReadUrl(string url)
    {
        var separator = url.IndexOf('?', StringComparison.Ordinal);
        var path = url[..separator];
        var query = QueryHelpers.ParseQuery(url[separator..]);

        var prefix = $"{BaseUrl}/{ILocalFileTransferService.ContentPathSegment}/";

        return (
            path[prefix.Length..],
            long.Parse(query["expires"].ToString()),
            query["signature"].ToString());
    }

    [Fact]
    public async Task UploadUrl_PointsAtTheContentEndpointAndCarriesASignature()
    {
        var url = await CreateService().CreatePresignedUploadUrlAsync(StoredPath);

        Assert.StartsWith($"{BaseUrl}/{ILocalFileTransferService.ContentPathSegment}/{StoredPath}?", url, StringComparison.Ordinal);

        var (path, expires, signature) = ReadUrl(url);
        Assert.Equal(StoredPath, path);
        Assert.NotEmpty(signature);
        Assert.Equal(
            new DateTimeOffset(_clock.UtcNow).AddMinutes(15).ToUnixTimeSeconds(),
            expires);
    }

    [Fact]
    public async Task IssuedSignature_IsAccepted()
    {
        var service = CreateService();
        var (path, expires, signature) = ReadUrl(await service.CreatePresignedUploadUrlAsync(StoredPath));

        Assert.True(service.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, signature));
    }

    [Fact]
    public async Task UploadSignature_IsNotAcceptedAsADownloadSignature()
    {
        // The operation is part of the signed payload for exactly this reason: a URL handed out so a
        // client can write one object must not also let it read that object back.
        var service = CreateService();
        var (path, expires, signature) = ReadUrl(await service.CreatePresignedUploadUrlAsync(StoredPath));

        Assert.False(service.IsSignatureValid(ILocalFileTransferService.DownloadOperation, path, expires, signature));
    }

    [Fact]
    public async Task SignatureForAnotherPath_IsRejected()
    {
        var service = CreateService();
        var (_, expires, signature) = ReadUrl(await service.CreatePresignedUploadUrlAsync(StoredPath));

        Assert.False(service.IsSignatureValid(
            ILocalFileTransferService.UploadOperation, "temp/somebody-elses-file.txt", expires, signature));
    }

    [Fact]
    public async Task TamperedSignature_IsRejected()
    {
        var service = CreateService();
        var (path, expires, signature) = ReadUrl(await service.CreatePresignedUploadUrlAsync(StoredPath));

        Assert.False(service.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, signature + "x"));
        Assert.False(service.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, null));
        Assert.False(service.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, string.Empty));
    }

    [Fact]
    public async Task ExtendingTheExpiry_DoesNotMakeASignatureValid()
    {
        var service = CreateService();
        var (path, expires, signature) = ReadUrl(await service.CreatePresignedUploadUrlAsync(StoredPath));

        Assert.False(service.IsSignatureValid(
            ILocalFileTransferService.UploadOperation, path, expires + 3600, signature));
    }

    [Fact]
    public async Task SignatureIsRejectedOnceItsExpiryHasPassed()
    {
        var service = CreateService(expiryMinutes: 15);
        var (path, expires, signature) = ReadUrl(await service.CreatePresignedUploadUrlAsync(StoredPath));

        Assert.True(service.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, signature));

        _clock.Advance(TimeSpan.FromMinutes(16));

        Assert.False(service.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, signature));
    }

    [Fact]
    public async Task WithoutASigningKey_NoUrlIsHandedOut()
    {
        // Refusing is the point: an unsigned URL would let anyone who guessed a path read or
        // overwrite stored objects.
        var service = CreateService(signingKey: null);

        await Assert.ThrowsAsync<ConfigurationMissingException>(
            () => service.CreatePresignedUploadUrlAsync(StoredPath));
    }

    [Fact]
    public async Task SavedBytesCanBeReadBackAndAreSeenByExists()
    {
        var service = CreateService();
        var payload = new byte[] { 1, 2, 3, 4, 5 };

        Assert.False(await service.ExistsAsync(StoredPath));

        using (var source = new MemoryStream(payload))
        {
            await service.SaveAsync(StoredPath, source);
        }

        Assert.True(await service.ExistsAsync(StoredPath));

        var stored = await service.OpenReadAsync(StoredPath);
        Assert.NotNull(stored);

        await using var readable = stored!;
        using var buffer = new MemoryStream();
        await readable.CopyToAsync(buffer);
        Assert.Equal(payload, buffer.ToArray());
    }

    [Fact]
    public async Task ReadingAFileThatIsNotThere_ReturnsNull()
    {
        Assert.Null(await CreateService().OpenReadAsync("temp/never-written.txt"));
    }

    [Fact]
    public async Task PathClimbingOutOfTheRoot_IsRefused()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.ExistsAsync("../../etc/passwd"));
    }

    [Fact]
    public async Task PathIntoASiblingDirectorySharingTheRootsName_IsRefused()
    {
        // The regression this guards: comparing against the bare root accepted any directory whose
        // name merely started with it, so a root of ".../uploads" let ".../uploads-evil" through.
        var service = CreateService();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.ExistsAsync("../uploads-evil/secret.txt"));
    }

    [Fact]
    public async Task WritingOutsideTheRoot_IsRefusedBeforeAnythingIsCreated()
    {
        var service = CreateService();
        using var source = new MemoryStream([9]);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.SaveAsync("../uploads-evil/planted.txt", source));

        var sibling = Path.Combine(Directory.GetParent(_root)!.FullName, "uploads-evil");
        Assert.False(Directory.Exists(sibling));
    }
}
