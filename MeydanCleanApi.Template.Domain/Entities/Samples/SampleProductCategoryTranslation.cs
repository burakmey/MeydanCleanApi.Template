using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Samples;

/// <summary>
/// Holds localized text fields for a <see cref="SampleProductCategory"/> per culture code.
/// </summary>
public class SampleProductCategoryTranslation : BaseEntity<Guid>, ITranslatable
{
    /// <inheritdoc />
    public override Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the parent category entity identifier.
    /// </summary>
    public Guid SampleProductCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the ISO/BCP 47 culture code for this translation (e.g., "tr-TR", "en-US").
    /// </summary>
    public required string CultureCode { get; set; }

    /// <summary>
    /// Gets or sets the localized title of the category.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the localized description of the category.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the URL-friendly localized slug for SEO routing.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// Navigation property to parent sample product category.
    /// </summary>
    public SampleProductCategory? SampleProductCategory { get; set; }
}
