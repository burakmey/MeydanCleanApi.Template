using System.Linq.Expressions;
using MeydanCleanApi.Template.Application.Common.Models.Pagination;

namespace MeydanCleanApi.Template.Application.Abstractions.Repositories;

/// <summary>
/// Generic read repository contract for querying entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The domain entity type inheriting from <see cref="BaseEntity{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The primary key type of the entity (e.g., <see cref="Guid"/>, <see cref="int"/>).</typeparam>
/// <remarks>
/// Every method takes the same optional <c>includes</c> argument. Related data is never loaded unless
/// you ask for it, so a navigation property you forgot to include comes back empty rather than
/// throwing, and the API quietly reports no related records. List what the response actually needs:
/// <code>
/// includes: [product =&gt; product.Files]
/// </code>
/// </remarks>
public interface IReadRepository<T, TKey> where T : BaseEntity<TKey>
{
    /// <summary>
    /// Returns the entity matching the primary key, or <c>null</c> if not found.
    /// </summary>
    /// <param name="id">Primary key value.</param>
    /// <param name="enableTracking">If <c>true</c>, attaches entity to change tracker for update mutations; otherwise uses No-Tracking for performance.</param>
    /// <param name="includes">Related collections or references to load with the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<T?> GetByIdAsync(
        TKey id,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the entity matching the primary key, or throws <see cref="IdNotFoundException"/> if not found.
    /// </summary>
    /// <param name="id">Primary key value.</param>
    /// <param name="enableTracking">If <c>true</c>, attaches entity to change tracker for update mutations; otherwise uses No-Tracking.</param>
    /// <param name="includes">Related collections or references to load with the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<T> GetByIdOrThrowAsync(
        TKey id,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the first entity matching the specified predicate, or <c>null</c> if none match.
    /// </summary>
    /// <param name="predicate">Filter expression.</param>
    /// <param name="enableTracking">If <c>true</c>, attaches entity to change tracker; otherwise uses No-Tracking.</param>
    /// <param name="includes">Related collections or references to load with the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">Filter expression.</param>
    /// <param name="enableTracking">If <c>true</c>, attaches entities to change tracker; otherwise uses No-Tracking.</param>
    /// <param name="includes">Related collections or references to load with each row.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all entities of type <typeparamref name="T"/> (No-Tracking read).
    /// </summary>
    /// <param name="includes">Related collections or references to load with each row.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<T>> GetAllAsync(
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns entities whose primary keys are contained in <paramref name="ids"/>.
    /// </summary>
    /// <param name="ids">Collection of primary keys.</param>
    /// <param name="enableTracking">If <c>true</c>, attaches entities to change tracker; otherwise uses No-Tracking.</param>
    /// <param name="includes">Related collections or references to load with each row.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<List<T>> GetByIdsAsync(
        IEnumerable<TKey> ids,
        bool enableTracking = false,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a paged subset of entities matching optional predicate filters (No-Tracking read).
    /// </summary>
    /// <param name="request">Page number and page size.</param>
    /// <param name="predicate">Optional filter applied before paging.</param>
    /// <param name="includes">Related collections or references to load with each row.</param>
    /// <param name="configureQuery">
    /// Optional hook to shape the query further, mainly to apply a custom sort order.
    /// When no order is set, results are sorted newest first.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<PagedResponse<T>> GetPagedAsync(
        PagedRequest request,
        Expression<Func<T, bool>>? predicate = null,
        IReadOnlyList<Expression<Func<T, object>>>? includes = null,
        Func<IQueryable<T>, IQueryable<T>>? configureQuery = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if at least one entity satisfies the specified predicate.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Returns the total count of entities matching the predicate (or total table count if predicate is null).
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}
