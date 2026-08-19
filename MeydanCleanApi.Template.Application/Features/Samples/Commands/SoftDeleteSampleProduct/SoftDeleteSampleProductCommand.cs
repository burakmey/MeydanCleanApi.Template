using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.SoftDeleteSampleProduct;

/// <summary>
/// Deactivates a product without removing its row.
/// </summary>
/// <param name="Id">Product to deactivate.</param>
/// <remarks>
/// The row stays in the table with <c>IsActive = false</c>. A global query filter then hides it from
/// every ordinary read, so the product disappears from the API while its history, orders and files
/// stay intact.
/// </remarks>
public sealed record SoftDeleteSampleProductCommand(Guid Id)
    : IRequest<BaseResponse<SoftDeleteSampleProductCommandResponse>>;
