using MeydanCleanApi.Template.Application.Common.Models;
using MeydanCleanApi.Template.Application.Features.Samples.Commands.AttachSampleProductFile;
using MeydanCleanApi.Template.Application.Features.Samples.Commands.CreateSampleProduct;
using MeydanCleanApi.Template.Application.Features.Samples.Commands.DetachSampleProductFile;
using MeydanCleanApi.Template.Application.Features.Samples.Commands.SoftDeleteSampleProduct;
using MeydanCleanApi.Template.Application.Features.Samples.Queries.GetSampleProductsPaged;
using MeydanCleanApi.Template.WebApi.Controllers.Base;

namespace MeydanCleanApi.Template.WebApi.Controllers.Samples;

/// <summary>
/// Reference endpoints showing the full CQRS flow: controller to mediator to handler to repository.
/// </summary>
/// <remarks>
/// This is sample code. Delete this controller together with the Samples feature folder and the
/// SampleProduct entities once you start building your own domain.
/// </remarks>
public sealed class SampleProductsController : ApiControllerBase
{
    /// <summary>
    /// Returns a paged list of sample products.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<GetSampleProductsPagedQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged([FromQuery] GetSampleProductsPagedQuery request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a sample product and optionally stages a file upload for it.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyConstants.Admin)]
    [ProducesResponseType(typeof(BaseResponse<CreateSampleProductCommandResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateSampleProductCommand request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);

        return CreatedAtAction(nameof(GetPaged), new { id = result.Data!.ProductId }, result);
    }

    /// <summary>
    /// Deactivates a product. The row is kept, so its history and files survive.
    /// </summary>
    /// <remarks>
    /// A global query filter hides deactivated products from every read, so the product disappears
    /// from the API while its row, history and files stay in the database.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyConstants.Admin)]
    [ProducesResponseType(typeof(BaseResponse<SoftDeleteSampleProductCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new SoftDeleteSampleProductCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Attaches an already-uploaded file to a product in a given role (gallery image, datasheet, ...).
    /// </summary>
    /// <remarks>
    /// Upload the bytes first through <c>/api/v1/files/upload-url</c> and confirm them, then call this
    /// with the returned file id. Permission is decided by the product, not by the file.
    /// </remarks>
    [HttpPost("{id:guid}/files")]
    [Authorize(Policy = PolicyConstants.Admin)]
    [ProducesResponseType(typeof(BaseResponse<AttachSampleProductFileCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AttachFile(
        [FromRoute] Guid id, [FromBody] AttachSampleProductFileRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new AttachSampleProductFileCommand(id, request.FileId, request.Purpose, request.SortOrder);
        var result = await Mediator.Send(command, ct);

        return Ok(result);
    }

    /// <summary>
    /// Removes a file from a product and deletes the stored object.
    /// </summary>
    [HttpDelete("{id:guid}/files/{fileId:guid}")]
    [Authorize(Policy = PolicyConstants.Admin)]
    [ProducesResponseType(typeof(BaseResponse<DetachSampleProductFileCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetachFile([FromRoute] Guid id, [FromRoute] Guid fileId, CancellationToken ct)
    {
        var result = await Mediator.Send(new DetachSampleProductFileCommand(id, fileId), ct);
        return Ok(result);
    }
}
