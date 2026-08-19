namespace MeydanCleanApi.Template.Application.Abstractions.Repositories;

/// <summary>
/// Unit of Work contract for committing pending changes and grouping work into a transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending change-tracked entity mutations to the underlying database asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <remarks>
    /// A single call is already atomic: the database wraps it in its own transaction. You only need
    /// <see cref="ExecuteInTransactionAsync"/> when several separate steps must succeed or fail together.
    /// </remarks>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction, committing when it finishes
    /// and rolling back if it throws.
    /// </summary>
    /// <param name="operation">The work to perform. Call <see cref="SaveChangesAsync"/> inside it as needed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>
    /// <para>
    /// Use this when one handler changes the database and something outside it, such as deleting a
    /// stored file, and a half-finished result would leave the two out of step.
    /// </para>
    /// <para>
    /// There is deliberately no separate Begin/Commit/Rollback trio. Callers cannot forget to roll
    /// back, and the whole block can be retried safely when the connection drops mid-way.
    /// </para>
    /// </remarks>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
}
