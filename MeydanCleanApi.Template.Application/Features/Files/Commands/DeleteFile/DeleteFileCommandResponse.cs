namespace MeydanCleanApi.Template.Application.Features.Files.Commands.DeleteFile;

/// <summary>
/// Response payload returned after deleting a file entity and storage object.
/// </summary>
/// <param name="FileId">Unique ID of the deleted file entity.</param>
public sealed record DeleteFileCommandResponse(Guid FileId);
