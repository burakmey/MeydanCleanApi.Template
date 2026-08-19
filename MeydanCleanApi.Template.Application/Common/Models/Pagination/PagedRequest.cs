using MeydanCleanApi.Template.Application.Common.Constants;

namespace MeydanCleanApi.Template.Application.Common.Models.Pagination;

/// <summary>
/// Immutable pagination query parameters with built-in range clamping against <see cref="PaginationConstants"/>.
/// </summary>
/// <param name="PageNumber">Requested 1-based page index (defaults to <see cref="PaginationConstants.DefaultPageNumber"/>).</param>
/// <param name="PageSize">Requested page size (defaults to <see cref="PaginationConstants.DefaultPageSize"/>, max <see cref="PaginationConstants.MaxPageSize"/>).</param>
public record PagedRequest(
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize)
{
    /// <summary>
    /// Gets the clamped page index (minimum 1).
    /// </summary>
    public int PageNumber { get; init; } = PageNumber < 1 ? PaginationConstants.DefaultPageNumber : PageNumber;

    /// <summary>
    /// Gets the clamped page size (minimum 1, maximum <see cref="PaginationConstants.MaxPageSize"/>).
    /// </summary>
    public int PageSize { get; init; } = PageSize > PaginationConstants.MaxPageSize
        ? PaginationConstants.MaxPageSize
        : (PageSize < 1 ? PaginationConstants.DefaultPageSize : PageSize);

    /// <summary>
    /// Gets the calculated row offset to skip for EF Core pagination.
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}
