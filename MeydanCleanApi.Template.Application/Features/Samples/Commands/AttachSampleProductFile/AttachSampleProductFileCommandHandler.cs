using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Files;
using MeydanCleanApi.Template.Domain.Entities.Samples;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.AttachSampleProductFile;

/// <summary>
/// Handler linking an uploaded file to a product with a purpose.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AttachSampleProductFileCommandHandler"/> class.
/// </remarks>
public sealed class AttachSampleProductFileCommandHandler(
    IReadRepository<SampleProduct, Guid> productReadRepository,
    IReadRepository<FileEntity, Guid> fileReadRepository,
    IWriteRepository<SampleProductFile, Guid> attachmentWriteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AttachSampleProductFileCommand, BaseResponse<AttachSampleProductFileCommandResponse>>
{
    private readonly IReadRepository<SampleProduct, Guid> _productReadRepository = productReadRepository;
    private readonly IReadRepository<FileEntity, Guid> _fileReadRepository = fileReadRepository;
    private readonly IWriteRepository<SampleProductFile, Guid> _attachmentWriteRepository = attachmentWriteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<BaseResponse<AttachSampleProductFileCommandResponse>> Handle(
        AttachSampleProductFileCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. The product must exist. Authorization for this endpoint is about the product,
        //    not the file, which is the whole point of keeping ownership off FileEntity.
        var productExists = await _productReadRepository.AnyAsync(
            product => product.Id == request.SampleProductId, ct);

        if (!productExists)
        {
            throw IdNotFoundException.For<SampleProduct>();
        }

        // 2. The file must exist and have finished uploading. Attaching a Pending file would leave
        //    the product pointing at bytes that may never arrive.
        var file = await _fileReadRepository.GetByIdOrThrowAsync(request.FileId, ct: ct);

        if (file.FileStatusId != (int)FileStatusType.Uploaded)
        {
            throw ConflictException.WithCode(ErrorCodes.FileNotUploaded);
        }

        // 3. Create the link. The attachment shares its id with the file, so attaching the same file
        //    twice hits the primary key and comes back as a 409 rather than silently duplicating.
        var attachment = new SampleProductFile
        {
            Id = file.Id,
            FileEntityId = file.Id,
            SampleProductId = request.SampleProductId,
            FilePurposeId = (int)request.Purpose,
            SortOrder = request.SortOrder
        };

        await _attachmentWriteRepository.AddAsync(attachment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return BaseResponse<AttachSampleProductFileCommandResponse>.Success(
            new AttachSampleProductFileCommandResponse(request.SampleProductId, file.Id));
    }
}
