using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Common.Models.FileStorage;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;

/// <summary>
/// Command handler for creating a pending file record in DB and returning a presigned upload URL.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreatePendingFileCommandHandler"/> class.
/// </remarks>
public sealed class CreatePendingFileCommandHandler(
    IFileStorageCoordinator coordinator,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePendingFileCommand, BaseResponse<CreatePendingFileCommandResponse>>
{
    private readonly IFileStorageCoordinator _coordinator = coordinator;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<BaseResponse<CreatePendingFileCommandResponse>> Handle(CreatePendingFileCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. A file is not owned by anybody on its own. It becomes reachable only once an
        //    attachment row links it to an entity, and that entity decides who may touch it.
        _ = _currentUserService.UserId ?? throw UnauthorizedException.WithCode();

        // 2. Stage the metadata row. The coordinator validates the container.
        var uploadModel = new FileUploadModel
        {
            OriginalFileName = request.OriginalFileName,
            ContentType = request.ContentType,
            SizeInBytes = request.SizeInBytes
        };

        var fileEntity = await _coordinator.CreatePendingAsync(uploadModel, request.Container, ct: ct);

        // 3. Save first, so a failure here means no presigned URL was ever handed out.
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Issue the short-lived URL the client uploads the bytes to.
        var uploadUrl = await _coordinator.CreatePresignedUploadUrlAsync(fileEntity, ct);

        return BaseResponse<CreatePendingFileCommandResponse>.Success(
            new CreatePendingFileCommandResponse(fileEntity.Id, uploadUrl));
    }
}
