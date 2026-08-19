using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Base;

/// <summary>
/// Abstract base class for all domain entities, implementing primary key and audit metadata properties.
/// </summary>
/// <typeparam name="TKey">The primary key data type for the entity (e.g., <see cref="Guid"/> or <see cref="int"/>).</typeparam>
public abstract class BaseEntity<TKey> : IBaseEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier of the entity.
    /// </summary>
    public abstract TKey Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created (stored in UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated (stored in UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
