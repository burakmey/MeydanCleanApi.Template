using System.Linq.Expressions;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Common.Models.Pagination;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Repositories;

/// <summary>
/// Generic EF Core read repository implementation of <see cref="IReadRepository{T, TKey}"/>.
/// Uses AsNoTracking by default for maximum query performance. Methods are marked virtual to allow custom repository override extension.
/// </summary>
/// <typeparam name="T">The entity type inheriting from <see cref="BaseEntity{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
public class ReadRepository<T, TKey> : IReadRepository<T, TKey> where T : BaseEntity<TKey>
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadRepository{T, TKey}"/> class.
    /// </summary>
    /// <param name="context">Injected application database context.</param>
    public ReadRepository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(
        TKey id,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        if (id is null) return null;

        var query = BuildQuery(enableTracking, includes);
        return await query.FirstOrDefaultAsync(x => x.Id!.Equals(id), ct);
    }

    /// <inheritdoc />
    public virtual async Task<T> GetByIdOrThrowAsync(
        TKey id,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, enableTracking, includes, ct);
        return entity ?? throw IdNotFoundException.For<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var query = BuildQuery(enableTracking, includes);
        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var query = BuildQuery(enableTracking, includes);
        return await query.Where(predicate).ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> GetAllAsync(
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        var query = BuildQuery(enableTracking: false, includes);
        return await query.ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> GetByIdsAsync(
        IEnumerable<TKey> ids,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var idList = ids.ToList();
        if (idList.Count == 0) return [];

        var query = BuildQuery(enableTracking, includes);
        return await query.Where(x => idList.Contains(x.Id)).ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<PagedResponse<T>> GetPagedAsync(
        PagedRequest request,
        Expression<Func<T, bool>>? predicate = null,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        Func<IQueryable<T>, IQueryable<T>>? configureQuery = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = BuildQuery(enableTracking: false, includes);

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (configureQuery is not null)
        {
            query = configureQuery(query);
        }

        var totalCount = await query.CountAsync(ct);

        // Skip/Take without an ORDER BY has no defined result in PostgreSQL: the same row can appear
        // on two pages, or on none. Sort newest first, then by Id to break ties between rows created
        // in the same instant. Pass configureQuery if a feature needs a different order; the ordering
        // applied there wins because it is applied first.
        if (query.Expression.Type != typeof(IOrderedQueryable<T>))
        {
            query = query.OrderByDescending(entity => entity.CreatedAt).ThenBy(entity => entity.Id);
        }

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
    }

    /// <summary>
    /// Starts a query with tracking and eager loading applied.
    /// </summary>
    /// <param name="enableTracking">Track the results for later updates, or read them without tracking.</param>
    /// <param name="includes">Related collections or references to load with each row.</param>
    /// <remarks>
    /// Shared by every read method so tracking and includes behave the same way everywhere.
    /// Soft-deleted rows are excluded automatically by the global query filter on the DbContext.
    /// </remarks>
    private IQueryable<T> BuildQuery(bool enableTracking, IReadOnlyList<Expression<Func<T, object>>>? includes)
    {
        var query = enableTracking ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();

        if (includes is not null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return query;
    }
}
