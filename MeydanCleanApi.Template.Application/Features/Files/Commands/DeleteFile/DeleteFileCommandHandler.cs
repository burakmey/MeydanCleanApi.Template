using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Common.Security;
using MeydanCleanApi.Template.Domain.Entities.Files;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.DeleteFile;

/// <summary>
/// Command handler for removing a file object from cloud storage and deleting its database record.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteFileCommandHandler"/> class.
/// </remarks>
public sealed class DeleteFileCommandHandler(
    IReadRepository<FileEntity, Guid> fileReadRepository,
    IWriteRepository<FileEntity, Guid> fileWriteRepository,
    IFileStorageCoordinator coordinator,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteFileCommand, BaseResponse<DeleteFileCommandResponse>>
{
    private readonly IReadRepository<FileEntity, Guid> _fileReadRepository = fileReadRepository;
    private readonly IWriteRepository<FileEntity, Guid> _fileWriteRepository = fileWriteRepository;
    private readonly IFileStorageCoordinator _coordinator = coordinator;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<BaseResponse<DeleteFileCommandResponse>> Handle(DeleteFileCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Load the record. This endpoint is for administrators clearing up orphaned uploads;
        //    files that belong to an entity are removed through that entity instead.
        var fileEntity = await _fileReadRepository.GetByIdOrThrowAsync(request.Id, enableTracking: true, ct: ct);

        // 2. Delete the database row first, then the stored object, both inside one transaction.
        //    Order matters: removing the stored file is permanent and cannot be undone, so it goes
        //    last. If the row cannot be deleted because something still references it, that surfaces
        //    as a 409 and the stored file is left untouched.
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            _fileWriteRepository.HardDelete(fileEntity);
            await _unitOfWork.SaveChangesAsync(token);

            await _coordinator.DeleteFromStorageAsync(fileEntity, token);
        }, ct);

        return BaseResponse<DeleteFileCommandResponse>.Success(new DeleteFileCommandResponse(fileEntity.Id));
    }
}
