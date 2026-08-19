using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.ConfirmFileUpload;

/// <summary>
/// Command payload for confirming that a pending file object exists in cloud storage and updating its status to Uploaded.
/// </summary>
/// <param name="Id">Unique primary key ID of the target file entity.</param>
public sealed record ConfirmFileUploadCommand(Guid Id) : IRequest<BaseResponse<ConfirmFileUploadCommandResponse>>;
