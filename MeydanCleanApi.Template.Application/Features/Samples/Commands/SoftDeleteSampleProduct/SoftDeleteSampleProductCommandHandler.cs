using MeydanCleanApi.Template.Application.Abstractions.Localization;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Localization;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Samples;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.SoftDeleteSampleProduct;

/// <summary>
/// Handler deactivating a product.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SoftDeleteSampleProductCommandHandler"/> class.
/// </remarks>
public sealed class SoftDeleteSampleProductCommandHandler(
    IReadRepository<SampleProduct, Guid> productReadRepository,
    IWriteRepository<SampleProduct, Guid> productWriteRepository,
    IUnitOfWork unitOfWork,
    ILocalizationService<ApiMessages> localizer)
    : IRequestHandler<SoftDeleteSampleProductCommand, BaseResponse<SoftDeleteSampleProductCommandResponse>>
{
    private readonly IReadRepository<SampleProduct, Guid> _productReadRepository = productReadRepository;
    private readonly IWriteRepository<SampleProduct, Guid> _productWriteRepository = productWriteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILocalizationService<ApiMessages> _localizer = localizer;

    /// <inheritdoc />
    public async Task<BaseResponse<SoftDeleteSampleProductCommandResponse>> Handle(
        SoftDeleteSampleProductCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Load the product with tracking, because its IsActive flag is about to change.
        //    An already-deactivated product is hidden by the query filter, so this returns 404
        //    rather than deactivating it twice.
        var product = await _productReadRepository.GetByIdOrThrowAsync(request.Id, enableTracking: true, ct: ct);

        // 2. Flip the flag. SoftDelete refuses entities that do not implement ISoftDeletable,
        //    so this can never turn into a permanent delete by accident.
        _productWriteRepository.SoftDelete(product);
        await _unitOfWork.SaveChangesAsync(ct);

        var entityName = _localizer.Get(nameof(SampleProduct));
        var message = _localizer.Get(ResponseCodes.SoftDeleted, entityName);

        return BaseResponse<SoftDeleteSampleProductCommandResponse>.Success(
            new SoftDeleteSampleProductCommandResponse(product.Id), message);
    }
}
