using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.DeleteFile;

/// <summary>
/// Command payload for permanently removing a file from storage provider and database.
/// </summary>
/// <param name="Id">Unique primary key ID of the target file entity.</param>
public sealed record DeleteFileCommand(Guid Id) : IRequest<BaseResponse<DeleteFileCommandResponse>>;
