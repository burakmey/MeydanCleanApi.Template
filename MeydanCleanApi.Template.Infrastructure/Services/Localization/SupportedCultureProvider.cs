using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Culture;
using Microsoft.Extensions.Caching.Memory;

namespace MeydanCleanApi.Template.Infrastructure.Services.Localization;

/// <summary>
/// Infrastructure implementation of <see cref="ISupportedCultureProvider"/> reading the
/// SupportedCultures table, with memory caching in front of it.
/// </summary>
/// <remarks>
/// Cultures are looked up on nearly every request, but they change very rarely, so the result is
/// cached for a few minutes rather than queried each time. Call <see cref="RefreshCache"/> after
/// adding or disabling a culture to pick the change up immediately.
/// </remarks>
public sealed class SupportedCultureProvider(
    IReadRepository<SupportedCulture, int> cultureReadRepository,
    IMemoryCache cache) : ISupportedCultureProvider
{
    private const string CacheKey = "supported-cultures:active";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IReadRepository<SupportedCulture, int> _cultureReadRepository = cultureReadRepository;
    private readonly IMemoryCache _cache = cache;

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetActiveCodesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        // The soft-delete query filter already hides rows where IsActive is false.
        var cultures = await _cultureReadRepository.GetAllAsync(ct: ct);

        IReadOnlyList<string> codes = cultures.Count > 0
            ? [.. cultures.Select(culture => culture.CultureCode)]

            // Falling back keeps the API serving requests when the table has not been seeded yet,
            // which is the normal state on a freshly created database.
            : CultureConstants.SupportedCultures;

        _cache.Set(CacheKey, codes, CacheDuration);
        return codes;
    }

    /// <inheritdoc />
    public async Task<bool> IsSupportedAsync(string? cultureCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cultureCode)) return false;

        var codes = await GetActiveCodesAsync(ct);
        return codes.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public void RefreshCache() => _cache.Remove(CacheKey);
}
