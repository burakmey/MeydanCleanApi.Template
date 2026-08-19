namespace MeydanCleanApi.Template.Domain.Entities.Base;

/// <summary>
/// Abstract base class for domain entities that use <see cref="Guid"/> as their primary key type.
/// </summary>
public abstract class GuidEntity : BaseEntity<Guid>
{
    /// <summary>
    /// Gets or sets the primary key identifier of the entity.
    /// </summary>
    public override Guid Id { get; set; }
}
