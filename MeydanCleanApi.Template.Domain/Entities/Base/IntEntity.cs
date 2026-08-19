namespace MeydanCleanApi.Template.Domain.Entities.Base;

/// <summary>
/// Abstract base class for domain entities that use <see cref="int"/> as their primary key type.
/// </summary>
public abstract class IntEntity : BaseEntity<int>
{
    /// <summary>
    /// Gets or sets the primary key identifier of the entity.
    /// </summary>
    public override int Id { get; set; }
}
