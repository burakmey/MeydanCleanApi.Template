using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Culture;

/// <summary>
/// Represents a supported application culture/locale for multi-language features.
/// </summary>
public class SupportedCulture : IntEntity, ISoftDeletable
{
    /// <summary>
    /// Gets or sets the IETF language tag culture code (e.g., "tr-TR", "en-US").
    /// </summary>
    public required string CultureCode { get; set; }

    /// <summary>
    /// Gets or sets the human-readable display name for the culture (e.g., "Türkçe", "English").
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this culture is active and available for public content.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
