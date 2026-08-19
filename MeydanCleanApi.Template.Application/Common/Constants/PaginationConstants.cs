namespace MeydanCleanApi.Template.Application.Common.Constants;

/// <summary>
/// Centralized pagination constants and default limits.
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Default initial page index (1-based).
    /// </summary>
    public const int DefaultPageNumber = 1;

    /// <summary>
    /// Default number of items per page.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum allowable items per page to prevent expensive table scans.
    /// </summary>
    public const int MaxPageSize = 50;
}
