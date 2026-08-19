using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Samples;

/// <summary>
/// Reference sample entity demonstrating product specification attributes (e.g., Material, Weight, Origin).
/// </summary>
public class SampleProductAttribute : GuidEntity, ITranslatable<SampleProductAttributeTranslation>, ISortable
{
    /// <summary>
    /// Gets or sets the parent product foreign key identifier.
    /// </summary>
    public Guid SampleProductId { get; set; }

    /// <summary>
    /// Gets or sets the display order index.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the parent product navigation property.
    /// </summary>
    public SampleProduct? SampleProduct { get; set; }

    /// <summary>
    /// Gets or sets the localized attribute translation records.
    /// </summary>
    public ICollection<SampleProductAttributeTranslation> Translations { get; set; } = [];
}
