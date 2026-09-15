using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Common.Models;
using MeydanCleanApi.Template.Application.Features.Files.Commands.ConfirmFileUpload;
using MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;
using MeydanCleanApi.Template.Application.Features.Files.Commands.DeleteFile;
using MeydanCleanApi.Template.Application.Features.Files.Queries.GetFilesPaged;
using MeydanCleanApi.Template.WebApi.Controllers.Base;
using Microsoft.AspNetCore.StaticFiles;

namespace MeydanCleanApi.Template.WebApi.Controllers.Files;

/// <summary>
/// Upload endpoints, plus an administrative view over the raw file table.
/// </summary>
/// <remarks>
/// <para>
/// A file here is just stored bytes — it belongs to nobody until an attachment row links it to an
/// entity. So this controller only covers the upload half: get a URL, then confirm the bytes arrived.
/// </para>
/// <para>
/// <strong>Listing and deleting a thing's files happens on that thing</strong>, for example
/// <c>DELETE /api/v1/sampleproducts/{productId}/files/{fileId}</c>, where the product decides who is
/// allowed. The two endpoints below are for administrators clearing up uploads that were started and
/// never attached.
/// </para>
/// </remarks>
[Authorize]
public sealed class FilesController(
    ILocalFileTransferService localFileTransfer,
    IContentTypeProvider contentTypeProvider) : ApiControllerBase
{
    private readonly ILocalFileTransferService _localFileTransfer = localFileTransfer;
    private readonly IContentTypeProvider _contentTypeProvider = contentTypeProvider;

    /// <summary>
    /// Step 1 of direct-to-cloud upload flow: creates a Pending file record in DB and returns a presigned upload URL.
    /// </summary>
    [HttpPost("upload-url")]
    [ProducesResponseType(typeof(BaseResponse<CreatePendingFileCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateUploadUrl([FromBody] CreatePendingFileCommand request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Confirms that a pending file object exists in cloud storage and updates its status to Uploaded.
    /// </summary>
    [HttpPut("{id:guid}/confirm")]
    [ProducesResponseType(typeof(BaseResponse<ConfirmFileUploadCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConfirmFileUploadCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Administrative view of the raw file table. Mainly used to find abandoned Pending uploads.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyConstants.Admin)]
    [ProducesResponseType(typeof(BaseResponse<GetFilesPagedQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged([FromQuery] GetFilesPagedQuery request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an unattached file. Files belonging to an entity are removed through that entity.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyConstants.Admin)]
    [ProducesResponseType(typeof(BaseResponse<DeleteFileCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteFileCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Receives the bytes for a local-storage upload URL issued by <c>POST upload-url</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Anonymous on purpose: the signature in the query string is the credential. It is tied to this
    /// exact path and expires, so it grants nothing else and not for long. Requiring a bearer token
    /// as well would stop a browser from uploading straight from a form.
    /// </para>
    /// <para>
    /// This endpoint and its GET counterpart exist only for the local provider. With a cloud provider
    /// the presigned URL points at the provider and the bytes never reach this API, so a project that
    /// stores everything in the cloud can delete both.
    /// </para>
    /// </remarks>
    [HttpPut($"{ILocalFileTransferService.ContentPathSegment}/{{**path}}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadContent(
        [FromRoute] string path,
        [FromQuery] long expires,
        [FromQuery] string? signature,
        CancellationToken ct)
    {
        if (!_localFileTransfer.IsSignatureValid(ILocalFileTransferService.UploadOperation, path, expires, signature))
        {
            return Forbid();
        }

        await _localFileTransfer.SaveAsync(path, Request.Body, ct);
        return NoContent();
    }

    /// <summary>
    /// Serves a stored file for a local-storage download URL.
    /// </summary>
    /// <remarks>
    /// Anonymous for the same reason as the upload endpoint: the signed, expiring query string is what
    /// authorizes the request.
    /// </remarks>
    [HttpGet($"{ILocalFileTransferService.ContentPathSegment}/{{**path}}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadContent(
        [FromRoute] string path,
        [FromQuery] long expires,
        [FromQuery] string? signature,
        CancellationToken ct)
    {
        if (!_localFileTransfer.IsSignatureValid(ILocalFileTransferService.DownloadOperation, path, expires, signature))
        {
            return Forbid();
        }

        var stream = await _localFileTransfer.OpenReadAsync(path, ct);
        if (stream is null)
        {
            return NotFound();
        }

        // Guessed from the extension rather than read from the file table: this endpoint is reached by
        // path, and a wrong content type here only affects how the browser displays the bytes.
        var contentType = _contentTypeProvider.TryGetContentType(path, out var resolved) && resolved is not null
            ? resolved
            : "application/octet-stream";

        return File(stream, contentType);
    }
}
