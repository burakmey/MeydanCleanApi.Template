using MeydanCleanApi.Template.Application.Common.Models;
using MeydanCleanApi.Template.Application.Features.Files.Commands.ConfirmFileUpload;
using MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;
using MeydanCleanApi.Template.Application.Features.Files.Commands.DeleteFile;
using MeydanCleanApi.Template.Application.Features.Files.Queries.GetFilesPaged;
using MeydanCleanApi.Template.WebApi.Controllers.Base;

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
public sealed class FilesController : ApiControllerBase
{
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
}
