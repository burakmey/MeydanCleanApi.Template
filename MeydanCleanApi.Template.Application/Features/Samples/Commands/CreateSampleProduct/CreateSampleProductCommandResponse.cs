namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.CreateSampleProduct;

/// <summary>
/// Command response containing the newly created product ID and presigned upload URL (if file attached).
/// </summary>
/// <param name="ProductId">Newly created product unique identifier.</param>
/// <param name="PresignedUploadUrl">Presigned URL for direct-to-cloud file upload (or null if no file attached).</param>
public record CreateSampleProductCommandResponse(
    Guid ProductId,
    string? PresignedUploadUrl = null);
