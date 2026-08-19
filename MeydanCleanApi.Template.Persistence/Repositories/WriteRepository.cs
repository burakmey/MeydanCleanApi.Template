using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Repositories;

/// <summary>
/// Generic EF Core write repository implementation of <see cref="IWriteRepository{T, TKey}"/>.
/// Methods are marked virtual to allow custom repository override extension.
/// </summary>
/// <typeparam name="T">The entity type inheriting from <see cref="BaseEntity{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
public class WriteRepository<T, TKey> : IWriteRepository<T, TKey> where T : BaseEntity<TKey>
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteRepository{T, TKey}"/> class.
    /// </summary>
    /// <param name="context">Injected application database context.</param>
    public WriteRepository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, ct);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities, ct);
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void SoftDelete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Refuse rather than quietly deleting the row for good. A method called SoftDelete must never
        // destroy data: a caller who wanted that would have called HardDelete instead.
        // The entity name is passed as a message argument so the localized text can name it.
        if (entity is not ISoftDeletable softDeletable)
        {
            throw InternalServerException.WithCode(ErrorCodes.SoftDeleteNotSupported, typeof(T).Name);
        }

        softDeletable.IsActive = false;
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void SoftDeleteRange(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            SoftDelete(entity);
        }
    }

    /// <inheritdoc />
    public virtual void HardDelete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public virtual void HardDeleteRange(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
    }
}
