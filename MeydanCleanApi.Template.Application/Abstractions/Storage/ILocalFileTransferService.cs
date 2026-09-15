namespace MeydanCleanApi.Template.Application.Abstractions.Storage;

/// <summary>
/// Contract for the endpoints that back local storage presigned URLs.
/// </summary>
/// <remarks>
/// <para>
/// A cloud provider receives the bytes itself, so the API only hands out a URL. Local storage has no
/// such provider: the signed URL points back at this API, and these members are what the endpoint
/// behind it uses to check the signature and move the bytes.
/// </para>
/// <para>
/// Only the local provider implements this. A project that stores everything in the cloud can delete
/// the transfer endpoints and this interface with it.
/// </para>
/// </remarks>
public interface ILocalFileTransferService
{
    /// <summary>
    /// Path segment that separates transfer URLs from the ordinary file endpoints.
    /// </summary>
    /// <remarks>
    /// Shared so the provider that builds the URL and the endpoint that answers it cannot drift apart.
    /// </remarks>
    const string ContentPathSegment = "content";

    /// <summary>The operation name signed into an upload URL.</summary>
    const string UploadOperation = "upload";

    /// <summary>The operation name signed into a download URL.</summary>
    const string DownloadOperation = "download";

    /// <summary>
    /// Returns whether the signature matches the path and is still within its expiry window.
    /// </summary>
    /// <param name="operation">Either <see cref="UploadOperation"/> or <see cref="DownloadOperation"/>.</param>
    /// <param name="path">Relative storage path taken from the request URL.</param>
    /// <param name="expiresAtUnixSeconds">Expiry stamp taken from the request query string.</param>
    /// <param name="signature">Signature taken from the request query string.</param>
    /// <remarks>
    /// The operation is part of the signed payload, so an upload URL cannot be replayed as a download
    /// URL or the other way round.
    /// </remarks>
    bool IsSignatureValid(string operation, string path, long expiresAtUnixSeconds, string? signature);

    /// <summary>
    /// Writes <paramref name="content"/> to <paramref name="path"/>, replacing anything already there.
    /// </summary>
    /// <param name="path">Relative storage path.</param>
    /// <param name="content">The incoming request body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(string path, Stream content, CancellationToken ct = default);

    /// <summary>
    /// Opens the stored file for reading, or returns <c>null</c> when it does not exist.
    /// </summary>
    /// <param name="path">Relative storage path.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Stream?> OpenReadAsync(string path, CancellationToken ct = default);
}
