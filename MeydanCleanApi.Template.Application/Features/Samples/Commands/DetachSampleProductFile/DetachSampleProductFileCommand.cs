using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.DetachSampleProductFile;

/// <summary>
/// Removes a file from a product and deletes the stored object.
/// </summary>
/// <param name="SampleProductId">Product the file is attached to.</param>
/// <param name="FileId">File to remove.</param>
public sealed record DetachSampleProductFileCommand(
    Guid SampleProductId,
    Guid FileId
) : IRequest<BaseResponse<DetachSampleProductFileCommandResponse>>;
