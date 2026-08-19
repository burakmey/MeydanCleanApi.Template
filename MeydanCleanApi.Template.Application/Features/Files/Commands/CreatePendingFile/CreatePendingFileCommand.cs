using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;

/// <summary>
/// Command payload for staging a pending file metadata record and issuing a presigned upload URL.
/// </summary>
/// <param name="OriginalFileName">Original filename including extension.</param>
/// <param name="ContentType">MIME content type (e.g. image/png).</param>
/// <param name="SizeInBytes">File size in bytes.</param>
/// <param name="Container">Storage container root. Must be one of <see cref="Constants.FileStorage.FileStorageContainers.All"/>.</param>
/// <remarks>
/// There is no sub-folder parameter. The container decides where the object goes and the file name
/// is generated server-side, so a caller cannot influence the storage path.
/// </remarks>
public sealed record CreatePendingFileCommand(
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string Container
) : IRequest<BaseResponse<CreatePendingFileCommandResponse>>;
