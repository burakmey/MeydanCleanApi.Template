namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Centralized numeric constraints, string length ceilings, and validation thresholds.
/// Eliminates hardcoded magic numbers across validators and EF Core entity configurations.
/// </summary>
public static class ValidationConstants
{
    /// <summary>Maximum character length for lookup entity names and short codes (50 characters).</summary>
    public const int MaxShortNameLength = 50;

    /// <summary>Maximum character length for culture codes (10 characters, e.g., "tr-TR").</summary>
    public const int MaxCultureCodeLength = 10;

    /// <summary>Common maximum character length for titles, attribute keys, and short names (100 characters).</summary>
    public const int MaxTitleLength = 100;

    /// <summary>Common maximum character length for category names (150 characters).</summary>
    public const int MaxCategoryNameLength = 150;

    /// <summary>Common maximum character length for entity names (200 characters).</summary>
    public const int MaxNameLength = 200;

    /// <summary>Common maximum character length for URL slugs (250 characters).</summary>
    public const int MaxSlugLength = 250;

    /// <summary>Maximum character length for OAuth provider keys and subject identifiers (256 characters).</summary>
    public const int MaxProviderKeyLength = 256;

    /// <summary>Maximum character length for file names (260 characters).</summary>
    public const int MaxFileNameLength = 260;

    /// <summary>Maximum character length for user emails (256 characters).</summary>
    public const int MaxEmailLength = 256;

    /// <summary>Maximum character length for product attribute values (500 characters).</summary>
    public const int MaxAttributeValueLength = 500;

    /// <summary>Maximum character length for security tokens, JWT hashes, and claims (500 characters).</summary>
    public const int MaxTokenLength = 500;

    /// <summary>Maximum character length for file/URL paths (1000 characters).</summary>
    public const int MaxPathLength = 1000;

    /// <summary>Common maximum character length for descriptions (2000 characters).</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>Minimum price threshold (0.01).</summary>
    public const decimal MinPrice = 0.01m;
}
