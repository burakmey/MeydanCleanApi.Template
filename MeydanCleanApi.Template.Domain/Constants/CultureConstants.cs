namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Centralized culture codes and display names for supported localization locales.
/// </summary>
public static class CultureConstants
{
    /// <summary>Turkish (Turkey) culture code.</summary>
    public const string TurkishCode = "tr-TR";

    /// <summary>Turkish (Turkey) human-readable display name.</summary>
    public const string TurkishDisplayName = "Türkçe";

    /// <summary>English (United States) culture code.</summary>
    public const string EnglishUsCode = "en-US";

    /// <summary>English (United States) human-readable display name.</summary>
    public const string EnglishUsDisplayName = "English (US)";

    /// <summary>Default system fallback culture code ("tr-TR").</summary>
    public const string DefaultCulture = TurkishCode;

    /// <summary>Set of active supported culture codes.</summary>
    public static readonly IReadOnlyList<string> SupportedCultures = [TurkishCode, EnglishUsCode];
}
