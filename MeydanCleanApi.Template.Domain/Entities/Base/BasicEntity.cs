namespace MeydanCleanApi.Template.Domain.Entities.Base;

/// <summary>
/// Minimal abstract base class for simple lookup entities with custom key type.
/// </summary>
/// <typeparam name="TKey">The primary key data type for the lookup entity.</typeparam>
public abstract class BasicEntity<TKey> : BaseEntity<TKey>
{
    /// <summary>
    /// Gets or sets the primary key identifier of the lookup entity.
    /// </summary>
    public override TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the unique display name or code of the lookup entity.
    /// </summary>
    public required string Name { get; set; }
}

/// <summary>
/// Minimal abstract base class for simple lookup entities with an integer primary key.
/// </summary>
public abstract class BasicEntity : BasicEntity<int>
{
}
