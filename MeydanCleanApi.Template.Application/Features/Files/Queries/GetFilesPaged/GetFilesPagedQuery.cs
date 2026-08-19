using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Common.Constants;
using MeydanCleanApi.Template.Application.Common.Models.Pagination;

namespace MeydanCleanApi.Template.Application.Features.Files.Queries.GetFilesPaged;

/// <summary>
/// Query payload for searching and retrieving a paged list of staged/uploaded file records.
/// </summary>
/// <param name="PageNumber">1-based page index.</param>
/// <param name="PageSize">Rows per page. Clamped to <see cref="PaginationConstants.MaxPageSize"/>.</param>
/// <param name="StatusId">Optional status filter (1=Pending, 2=Uploaded, 3=Failed).</param>
/// <param name="SearchTerm">Optional search term matching the original filename.</param>
/// <remarks>
/// Paging values are flat rather than nested so they bind from a plain query string,
/// for example <c>?pageNumber=2&amp;pageSize=20&amp;statusId=2</c>.
/// </remarks>
public sealed record GetFilesPagedQuery(
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize,
    int? StatusId = null,
    string? SearchTerm = null
) : IRequest<BaseResponse<GetFilesPagedQueryResponse>>
{
    /// <summary>Gets the paging values with the shared clamping rules already applied.</summary>
    public PagedRequest Page => new(PageNumber, PageSize);
}
