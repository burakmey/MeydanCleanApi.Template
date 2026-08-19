namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.SoftDeleteSampleProduct;

/// <summary>
/// Response returned after deactivating a product.
/// </summary>
/// <param name="Id">The product that was deactivated.</param>
public sealed record SoftDeleteSampleProductCommandResponse(Guid Id);
