using MeydanCleanApi.Template.Application.Common.Models.FileStorage;
using MeydanCleanApi.Template.Application.Common.Models.Pagination;

namespace MeydanCleanApi.Template.Application.Features.Files.Queries.GetFilesPaged;

/// <summary>
/// Response payload containing paged file DTO records.
/// </summary>
/// <param name="Files">Paged list of file items.</param>
public sealed record GetFilesPagedQueryResponse(
    PagedResponse<FileDto> Files
);
