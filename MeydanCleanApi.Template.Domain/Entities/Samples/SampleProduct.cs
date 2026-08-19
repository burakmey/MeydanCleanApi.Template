using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Samples;

/// <summary>
/// Reference sample master entity demonstrating multi-language support, soft deletion, attributes, attached files, and category relationships.
/// </summary>
public class SampleProduct : GuidEntity, ITranslatable<SampleProductTranslation>, ISoftDeletable
{
    /// <summary>
    /// Gets or sets the foreign key identifier of the category.
    /// </summary>
    public required Guid SampleProductCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the internal culture-invariant product code or name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the category navigation property.
    /// </summary>
    public SampleProductCategory? SampleProductCategory { get; set; }

    /// <summary>
    /// Gets or sets the collection of product specification attributes.
    /// </summary>
    public ICollection<SampleProductAttribute> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of attached file entities (gallery images, documents).
    /// </summary>
    public ICollection<SampleProductFile> Files { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of multi-language translations.
    /// </summary>
    public ICollection<SampleProductTranslation> Translations { get; set; } = [];
}
