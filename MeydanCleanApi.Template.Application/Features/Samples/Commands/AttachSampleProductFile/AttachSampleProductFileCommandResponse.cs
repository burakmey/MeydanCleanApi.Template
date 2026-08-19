namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.AttachSampleProductFile;

/// <summary>
/// Response returned after attaching a file to a product.
/// </summary>
/// <param name="SampleProductId">Product the file was attached to.</param>
/// <param name="FileId">File that is now attached. Also the attachment row's own identifier.</param>
public sealed record AttachSampleProductFileCommandResponse(Guid SampleProductId, Guid FileId);
