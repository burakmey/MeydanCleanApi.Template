using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Samples;

/// <summary>
/// Holds localized key-value attribute pair for a <see cref="SampleProductAttribute"/> per culture code.
/// </summary>
public class SampleProductAttributeTranslation : BaseEntity<Guid>, ITranslatable
{
    /// <inheritdoc />
    public override Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the parent attribute entity identifier.
    /// </summary>
    public Guid SampleProductAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the ISO/BCP 47 culture code for this translation (e.g., "tr-TR", "en-US").
    /// </summary>
    public required string CultureCode { get; set; }

    /// <summary>
    /// Gets or sets the localized attribute key (e.g., "Color", "Size").
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the localized attribute value (e.g., "Red", "XL").
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Navigation property to parent sample product attribute.
    /// </summary>
    public SampleProductAttribute? SampleProductAttribute { get; set; }
}
