using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Persistence.Contexts;
using Npgsql;

namespace MeydanCleanApi.Template.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for coordinating transactions and persisting changes.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">Injected application database context.</param>
    public UnitOfWork(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Somebody else changed the same row between our read and our write.
            // Report it as a conflict so the caller can reload and try again.
            throw ConflictException.WithCode();
        }
        catch (DbUpdateException ex) when (IsConstraintViolation(ex))
        {
            // A unique or foreign key constraint stopped the write. That is the database refusing an
            // operation the caller asked for, not a server fault, so report it as a conflict (409)
            // rather than letting it surface as an unexplained 500.
            throw ConflictException.WithCode();
        }
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // The execution strategy owns the retry loop. Opening the transaction inside it means a
        // dropped connection replays the whole block instead of leaving it half applied.
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                await operation(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }

    /// <summary>
    /// Returns whether the failure came from a unique, foreign key or check constraint.
    /// </summary>
    private static bool IsConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState is
                PostgresErrorCodes.UniqueViolation or
                PostgresErrorCodes.ForeignKeyViolation or
                PostgresErrorCodes.CheckViolation;
    }
}
