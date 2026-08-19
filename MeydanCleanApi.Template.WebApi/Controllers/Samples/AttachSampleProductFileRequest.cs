namespace MeydanCleanApi.Template.WebApi.Controllers.Samples;

/// <summary>
/// Body of <c>POST /api/v1/sampleproducts/{id}/files</c>.
/// </summary>
/// <param name="FileId">File that was uploaded and confirmed beforehand.</param>
/// <param name="Purpose">Role the file plays for this product.</param>
/// <param name="SortOrder">Position within the same purpose.</param>
/// <remarks>
/// A small web-only shape, because the product id belongs in the route rather than the body. The
/// controller combines the two into the command.
/// </remarks>
public sealed record AttachSampleProductFileRequest(
    Guid FileId,
    FilePurposeType Purpose,
    int SortOrder = 0);
