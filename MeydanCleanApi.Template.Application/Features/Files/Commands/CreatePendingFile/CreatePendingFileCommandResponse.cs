namespace MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;

/// <summary>
/// Response payload returned after creating a pending file record.
/// </summary>
/// <param name="FileId">The unique ID assigned to the staged file record.</param>
/// <param name="UploadUrl">The presigned upload URL generated for direct storage upload.</param>
/// <remarks>
/// The internal storage path is not returned. The client only needs the id to confirm the upload
/// afterwards, and the URL to send the bytes to.
/// </remarks>
public sealed record CreatePendingFileCommandResponse(
    Guid FileId,
    string UploadUrl
);
