using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Common.Constants;
using MeydanCleanApi.Template.Application.Common.Models.Pagination;

namespace MeydanCleanApi.Template.Application.Features.Samples.Queries.GetSampleProductsPaged;

/// <summary>
/// CQRS Query for retrieving a paged collection of sample products with optional search filter.
/// </summary>
/// <param name="PageNumber">1-based page index.</param>
/// <param name="PageSize">Rows per page. Clamped to <see cref="PaginationConstants.MaxPageSize"/>.</param>
/// <param name="SearchTerm">Optional search term for filtering products by name.</param>
/// <remarks>
/// Paging values are flat rather than nested so they bind from a plain query string,
/// for example <c>?pageNumber=2&amp;pageSize=20</c>.
/// </remarks>
public sealed record GetSampleProductsPagedQuery(
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize,
    string? SearchTerm = null) : IRequest<BaseResponse<GetSampleProductsPagedQueryResponse>>
{
    /// <summary>Gets the paging values with the shared clamping rules already applied.</summary>
    public PagedRequest Page => new(PageNumber, PageSize);
}
