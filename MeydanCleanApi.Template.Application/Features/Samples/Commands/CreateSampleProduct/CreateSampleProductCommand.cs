using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Common.Models.FileStorage;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.CreateSampleProduct;

/// <summary>
/// CQRS Command for creating a new sample product entity and optional file attachment.
/// </summary>
/// <param name="SampleProductCategoryId">Category the product belongs to. Must already exist.</param>
/// <param name="Name">Product display name.</param>
/// <param name="Price">Unit price.</param>
/// <param name="MainFile">Optional file metadata payload for direct-to-cloud upload staging.</param>
public record CreateSampleProductCommand(
    Guid SampleProductCategoryId,
    string Name,
    decimal Price,
    FileUploadModel? MainFile = null) : IRequest<BaseResponse<CreateSampleProductCommandResponse>>;
