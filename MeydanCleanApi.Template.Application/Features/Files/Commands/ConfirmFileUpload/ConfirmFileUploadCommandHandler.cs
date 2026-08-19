using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Common.Security;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Files;
using MeydanCleanApi.Template.Domain.Exceptions;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.ConfirmFileUpload;

/// <summary>
/// Command handler for manually verifying file existence in cloud storage and setting its status to Uploaded.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConfirmFileUploadCommandHandler"/> class.
/// </remarks>
public sealed class ConfirmFileUploadCommandHandler(
    IReadRepository<FileEntity, Guid> fileReadRepository,
    IFileStorageCoordinator coordinator,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmFileUploadCommand, BaseResponse<ConfirmFileUploadCommandResponse>>
{
    private readonly IReadRepository<FileEntity, Guid> _fileReadRepository = fileReadRepository;
    private readonly IFileStorageCoordinator _coordinator = coordinator;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<BaseResponse<ConfirmFileUploadCommandResponse>> Handle(ConfirmFileUploadCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Load the record. Tracking is on because the status is about to change.
        var fileEntity = await _fileReadRepository.GetByIdOrThrowAsync(request.Id, enableTracking: true, ct: ct);

        // 2. No ownership check here. A pending file is not attached to anything yet, so there is
        //    nothing to own. Confirming only asks storage whether the bytes arrived.

        // 3. Ask storage whether the bytes actually arrived, and flip the status if so.
        var confirmed = await _coordinator.ConfirmUploadedAsync(fileEntity, ct);
        if (!confirmed)
        {
            throw ConflictException.WithCode(ErrorCodes.FileNotUploaded);
        }

        // 4. Persist the new status.
        await _unitOfWork.SaveChangesAsync(ct);

        return BaseResponse<ConfirmFileUploadCommandResponse>.Success(new ConfirmFileUploadCommandResponse(fileEntity.Id));
    }
}
