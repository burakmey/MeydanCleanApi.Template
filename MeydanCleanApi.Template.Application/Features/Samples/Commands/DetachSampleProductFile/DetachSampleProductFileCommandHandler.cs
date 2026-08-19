using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Domain.Entities.Files;
using MeydanCleanApi.Template.Domain.Entities.Samples;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.DetachSampleProductFile;

/// <summary>
/// Handler removing a product's file: the link, the metadata row and the stored object.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DetachSampleProductFileCommandHandler"/> class.
/// </remarks>
public sealed class DetachSampleProductFileCommandHandler(
    IReadRepository<SampleProductFile, Guid> attachmentReadRepository,
    IWriteRepository<SampleProductFile, Guid> attachmentWriteRepository,
    IReadRepository<FileEntity, Guid> fileReadRepository,
    IWriteRepository<FileEntity, Guid> fileWriteRepository,
    IFileStorageCoordinator coordinator,
    IUnitOfWork unitOfWork) : IRequestHandler<DetachSampleProductFileCommand, BaseResponse<DetachSampleProductFileCommandResponse>>
{
    private readonly IReadRepository<SampleProductFile, Guid> _attachmentReadRepository = attachmentReadRepository;
    private readonly IWriteRepository<SampleProductFile, Guid> _attachmentWriteRepository = attachmentWriteRepository;
    private readonly IReadRepository<FileEntity, Guid> _fileReadRepository = fileReadRepository;
    private readonly IWriteRepository<FileEntity, Guid> _fileWriteRepository = fileWriteRepository;
    private readonly IFileStorageCoordinator _coordinator = coordinator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<BaseResponse<DetachSampleProductFileCommandResponse>> Handle(
        DetachSampleProductFileCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Find the link, matching on both ids. Checking the product too means a caller cannot
        //    detach a file from a product it does not belong to by guessing the file id.
        var attachment = await _attachmentReadRepository.FirstOrDefaultAsync(
            link => link.FileEntityId == request.FileId && link.SampleProductId == request.SampleProductId,
            enableTracking: true,
            ct: ct)
            ?? throw IdNotFoundException.For<SampleProductFile>();

        var fileEntity = await _fileReadRepository.GetByIdOrThrowAsync(request.FileId, enableTracking: true, ct: ct);

        // 2. Remove the link, then the file row, then the stored object. Deleting the stored bytes is
        //    permanent, so it happens last and only once the database side has succeeded.
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _attachmentWriteRepository.HardDelete(attachment);
            _fileWriteRepository.HardDelete(fileEntity);
            await _unitOfWork.SaveChangesAsync(token);

            await _coordinator.DeleteFromStorageAsync(fileEntity, token);
        }, ct);

        return BaseResponse<DetachSampleProductFileCommandResponse>.Success(
            new DetachSampleProductFileCommandResponse(request.FileId));
    }
}
