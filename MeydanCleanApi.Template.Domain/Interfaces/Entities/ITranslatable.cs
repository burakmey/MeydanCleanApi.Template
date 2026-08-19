namespace MeydanCleanApi.Template.Domain.Interfaces.Entities;

/// <summary>
/// Implemented by localized translation child entities (e.g., <c>SampleProductTranslation</c>).
/// </summary>
/// <remarks>
/// Enforces that every translation record holds a valid BCP 47 / ISO culture code (e.g., "tr-TR", "en-US").
/// </remarks>
public interface ITranslatable
{
    /// <summary>
    /// Gets or sets the culture code for the localized translation record.
    /// </summary>
    string CultureCode { get; set; }
}

/// <summary>
/// Implemented by main master domain entities (e.g., <c>SampleProduct</c>) that hold multi-language translations.
/// </summary>
/// <typeparam name="TTranslation">The translation child entity type implementing <see cref="ITranslatable"/>.</typeparam>
/// <remarks>
/// <para>
/// <strong>Master-Detail Localization Pattern:</strong>
/// The master entity owns a collection of translation records, allowing fields like title, slug, and description
/// to be localized per culture without duplicating core domain properties (e.g., Price, Stock, CategoryId).
/// </para>
/// </remarks>
public interface ITranslatable<TTranslation>
{
    /// <summary>
    /// Gets or sets the collection of localized translation records for this entity.
    /// </summary>
    ICollection<TTranslation> Translations { get; set; }
}
