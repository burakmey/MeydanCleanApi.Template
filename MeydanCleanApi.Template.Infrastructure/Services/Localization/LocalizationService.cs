using Microsoft.Extensions.Localization;

namespace MeydanCleanApi.Template.Infrastructure.Services.Localization;

/// <summary>
/// Infrastructure implementation of <see cref="ILocalizationService{T}"/> using ASP.NET Core's <see cref="IStringLocalizer{T}"/>.
/// </summary>
/// <typeparam name="T">Localization marker type (e.g. ValidationMessages, ErrorMessages, ApiMessages).</typeparam>
public sealed class LocalizationService<T>(IStringLocalizer<T> localizer) : ILocalizationService<T>
{
    /// <inheritdoc />
    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var result = localizer[key];
        return result.ResourceNotFound ? key : result.Value;
    }

    /// <inheritdoc />
    public string Get(string key, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var result = localizer[key, args];
        return result.ResourceNotFound ? key : result.Value;
    }
}
