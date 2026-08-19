namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.DetachSampleProductFile;

/// <summary>
/// Response returned after detaching a file from a product.
/// </summary>
/// <param name="FileId">File that was removed.</param>
public sealed record DetachSampleProductFileCommandResponse(Guid FileId);
