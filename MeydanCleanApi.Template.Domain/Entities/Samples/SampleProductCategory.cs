using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Samples;

/// <summary>
/// Reference sample entity demonstrating hierarchical category structure and localization.
/// </summary>
public class SampleProductCategory : GuidEntity, ITranslatable<SampleProductCategoryTranslation>, ISoftDeletable, ISortable
{
    /// <summary>
    /// Gets or sets the parent category identifier. Null for top-level root categories.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the internal culture-invariant category name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the display order index.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the category is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the parent category navigation property.
    /// </summary>
    public SampleProductCategory? Parent { get; set; }

    /// <summary>
    /// Gets or sets the child subcategories collection.
    /// </summary>
    public ICollection<SampleProductCategory> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the products collection belonging to this category.
    /// </summary>
    public ICollection<SampleProduct> Products { get; set; } = [];

    /// <summary>
    /// Gets or sets the multi-language translations collection.
    /// </summary>
    public ICollection<SampleProductCategoryTranslation> Translations { get; set; } = [];
}
