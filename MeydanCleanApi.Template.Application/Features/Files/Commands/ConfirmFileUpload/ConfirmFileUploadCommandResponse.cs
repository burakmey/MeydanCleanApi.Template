namespace MeydanCleanApi.Template.Application.Features.Files.Commands.ConfirmFileUpload;

/// <summary>
/// Response payload returned after confirming a file upload.
/// </summary>
/// <param name="FileId">Unique ID of the confirmed file entity.</param>
public sealed record ConfirmFileUploadCommandResponse(Guid FileId);
