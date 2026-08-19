using MeydanCleanApi.Template.Application.Abstractions.Localization;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Constants.FileStorage;
using MeydanCleanApi.Template.Application.Localization;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Samples;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.CreateSampleProduct;

/// <summary>
/// Handler executing product creation and optional direct-to-cloud file attachment.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateSampleProductCommandHandler"/> class.
/// </remarks>
public sealed class CreateSampleProductCommandHandler(
    IReadRepository<SampleProductCategory, Guid> categoryReadRepository,
    IWriteRepository<SampleProduct, Guid> productWriteRepository,
    IWriteRepository<SampleProductFile, Guid> productFileWriteRepository,
    IUnitOfWork unitOfWork,
    IFileStorageCoordinator fileStorageCoordinator,
    ILocalizationService<ApiMessages> localizer) : IRequestHandler<CreateSampleProductCommand, BaseResponse<CreateSampleProductCommandResponse>>
{
    private readonly IReadRepository<SampleProductCategory, Guid> _categoryReadRepository = categoryReadRepository;
    private readonly IWriteRepository<SampleProduct, Guid> _productWriteRepository = productWriteRepository;
    private readonly IWriteRepository<SampleProductFile, Guid> _productFileWriteRepository = productFileWriteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IFileStorageCoordinator _fileStorageCoordinator = fileStorageCoordinator;
    private readonly ILocalizationService<ApiMessages> _localizer = localizer;

    /// <inheritdoc />
    public async Task<BaseResponse<CreateSampleProductCommandResponse>> Handle(CreateSampleProductCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Make sure the category exists. Inserting with an id that has no matching row would fail
        //    at the database with a foreign key error, which is much harder to read than a clean 404.
        var categoryExists = await _categoryReadRepository.AnyAsync(
            category => category.Id == request.SampleProductCategoryId, ct);

        if (!categoryExists)
        {
            throw IdNotFoundException.For<SampleProductCategory>();
        }

        // 2. Create the product.
        var product = new SampleProduct
        {
            Id = Guid.NewGuid(),
            SampleProductCategoryId = request.SampleProductCategoryId,
            Name = request.Name,
            Price = request.Price,
            IsActive = true
        };

        await _productWriteRepository.AddAsync(product, ct);

        string? presignedUploadUrl = null;

        if (request.MainFile is not null)
        {
            // 3. Stage pending file metadata using centralized FileStorageContainers constants
            var fileEntity = await _fileStorageCoordinator.CreatePendingAsync(
                request.MainFile,
                container: FileStorageContainers.Public.Products,
                subFolder: null,
                ct: ct);

            // 4. Link the file to the product, saying what it is for. Gallery images and a PDF manual
            //    are rows in this same table, told apart by the purpose.
            var productFile = new SampleProductFile
            {
                Id = fileEntity.Id,
                SampleProductId = product.Id,
                FileEntityId = fileEntity.Id,
                FilePurposeId = (int)FilePurposeType.Gallery,
                SortOrder = 1
            };

            await _productFileWriteRepository.AddAsync(productFile, ct);

            // 5. Generate presigned upload URL for direct-to-cloud client upload
            presignedUploadUrl = await _fileStorageCoordinator.CreatePresignedUploadUrlAsync(fileEntity, ct);
        }

        // 6. Save the product, the file record and the join row together.
        await _unitOfWork.SaveChangesAsync(ct);

        var responseData = new CreateSampleProductCommandResponse(product.Id, presignedUploadUrl);

        // Compile-time safe entity name resolution using C# nameof keyword (zero hardcoded strings)
        var entityName = _localizer.Get(nameof(SampleProduct));
        var localizedMessage = _localizer.Get(ResponseCodes.Created, entityName);

        return BaseResponse<CreateSampleProductCommandResponse>.Success(responseData, localizedMessage);
    }
}
