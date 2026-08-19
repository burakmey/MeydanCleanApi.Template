namespace MeydanCleanApi.Template.Domain.Interfaces.Entities;

/// <summary>
/// Defines the base contract for all domain entities, providing standard audit metadata properties.
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created (stored in UTC).
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated (stored in UTC).
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
