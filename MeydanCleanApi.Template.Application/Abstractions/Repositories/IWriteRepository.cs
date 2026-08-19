namespace MeydanCleanApi.Template.Application.Abstractions.Repositories;

/// <summary>
/// Generic write repository contract for entity mutations (Add, Update, SoftDelete, HardDelete).
/// </summary>
/// <typeparam name="T">The domain entity type inheriting from <see cref="BaseEntity{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The primary key type of the entity.</typeparam>
public interface IWriteRepository<T, TKey> where T : BaseEntity<TKey>
{
    /// <summary>
    /// Adds a new entity to the change tracker.
    /// </summary>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Adds a collection of new entities to the change tracker.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// Marks an existing entity as modified in the change tracker.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Soft-deletes an entity by marking <c>IsActive = false</c> (if entity implements <see cref="Domain.Interfaces.Entities.ISoftDeletable"/>).
    /// </summary>
    /// <param name="entity">Target entity instance.</param>
    void SoftDelete(T entity);

    /// <summary>
    /// Soft-deletes a collection of entities.
    /// </summary>
    /// <param name="entities">Target entity collection.</param>
    void SoftDeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Physically removes an entity row from the database table regardless of soft-delete interfaces.
    /// </summary>
    /// <param name="entity">Target entity instance.</param>
    void HardDelete(T entity);

    /// <summary>
    /// Physically removes a collection of entity rows from the database table.
    /// </summary>
    /// <param name="entities">Target entity collection.</param>
    void HardDeleteRange(IEnumerable<T> entities);
}
