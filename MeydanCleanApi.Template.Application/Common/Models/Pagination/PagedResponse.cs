namespace MeydanCleanApi.Template.Application.Common.Models.Pagination;

/// <summary>
/// Generic paged response payload containing items and navigation metadata.
/// </summary>
/// <typeparam name="T">The payload element type.</typeparam>
public record PagedResponse<T>
{
    /// <summary>
    /// Gets or sets the collection of items on the current page.
    /// </summary>
    public IEnumerable<T> Items { get; init; } = [];

    /// <summary>
    /// Gets or sets the total number of matching records across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets or sets the current 1-based page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets or sets the page size used for the query.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the calculated total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Projects each element in the current page to another type while preserving paging metadata.
    /// </summary>
    /// <typeparam name="TDestination">The target projection type (e.g., DTO).</typeparam>
    /// <param name="selector">Projection function applied to each element.</param>
    /// <returns>A new <see cref="PagedResponse{TDestination}"/> containing projected items.</returns>
    public PagedResponse<TDestination> Map<TDestination>(Func<T, TDestination> selector)
    {
        return new PagedResponse<TDestination>
        {
            Items = [.. Items.Select(selector)],
            TotalCount = TotalCount,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
