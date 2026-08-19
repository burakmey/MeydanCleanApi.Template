namespace MeydanCleanApi.Template.Application.Abstractions.Localization;

/// <summary>
/// Service contract providing access to system-supported ISO culture codes (e.g., "tr-TR", "en-US").
/// </summary>
public interface ISupportedCultureProvider
{
    /// <summary>
    /// Returns the collection of active supported ISO culture codes.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of active culture codes.</returns>
    Task<IReadOnlyList<string>> GetActiveCodesAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks whether the specified culture code is actively supported.
    /// </summary>
    /// <param name="cultureCode">ISO culture code string (case-insensitive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if supported; otherwise, <c>false</c>.</returns>
    Task<bool> IsSupportedAsync(string? cultureCode, CancellationToken ct = default);

    /// <summary>
    /// Invalidates the internal culture cache forcing a fresh database reload on the next query.
    /// </summary>
    void RefreshCache();
}
