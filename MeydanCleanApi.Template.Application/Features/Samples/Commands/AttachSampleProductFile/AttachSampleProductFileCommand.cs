using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.AttachSampleProductFile;

/// <summary>
/// Attaches an already-uploaded file to a product in a given role.
/// </summary>
/// <param name="SampleProductId">Product the file belongs to.</param>
/// <param name="FileId">File that was uploaded and confirmed beforehand.</param>
/// <param name="Purpose">Role the file plays: gallery image, datasheet, manual, and so on.</param>
/// <param name="SortOrder">Position within the same purpose. Lower numbers appear first.</param>
/// <remarks>
/// This is the second half of the upload flow. The client first calls the file endpoints to upload
/// the bytes, then calls this to say what the file is for. Splitting it that way means the upload can
/// be retried without touching the product.
/// </remarks>
public sealed record AttachSampleProductFileCommand(
    Guid SampleProductId,
    Guid FileId,
    FilePurposeType Purpose,
    int SortOrder = 0
) : IRequest<BaseResponse<AttachSampleProductFileCommandResponse>>;
