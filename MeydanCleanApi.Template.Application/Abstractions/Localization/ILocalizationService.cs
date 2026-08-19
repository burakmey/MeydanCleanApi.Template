namespace MeydanCleanApi.Template.Application.Abstractions.Localization;

/// <summary>
/// Generic abstraction for resolving localized strings from resx resources.
/// Decouples Application and Domain layers from ASP.NET Core's <c>IStringLocalizer</c>.
/// </summary>
/// <typeparam name="T">Resx resource marker class.</typeparam>
public interface ILocalizationService<T>
{
    /// <summary>
    /// Returns the localized string for the specified key. Falls back to the key if not found.
    /// </summary>
    /// <param name="key">Resource key name.</param>
    /// <returns>Localized string value.</returns>
    string Get(string key);

    /// <summary>
    /// Returns the localized string for the specified key formatted with positional arguments.
    /// </summary>
    /// <param name="key">Resource key name.</param>
    /// <param name="args">Format arguments.</param>
    /// <returns>Formatted localized string value.</returns>
    string Get(string key, params object[] args);
}
