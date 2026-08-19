using Microsoft.Extensions.Logging;
using MeydanCleanApi.Template.Persistence.Abstractions;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Seeding.Base;

/// <summary>
/// Provides an object-oriented base implementation for seeder services using the Template Method pattern and structured logging.
/// </summary>
/// <typeparam name="T">The type of the domain entity, which must implement <see cref="IBaseEntity"/>.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="BaseSeederService{T}"/> class.
/// </remarks>
/// <param name="context">The database context instance.</param>
/// <param name="logger">Logger instance.</param>
public abstract class BaseSeederService<T>(ApplicationDbContext context, ILogger logger) : ISeederService<T> where T : class, IBaseEntity
{
    /// <summary>
    /// The database context used for querying and writing seed data.
    /// </summary>
    protected readonly ApplicationDbContext Context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Logger instance for diagnostic output and audit tracking.
    /// </summary>
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken ct = default)
    {
        try
        {
            var newEntities = await GetNewEntitiesAsync(ct);

            if (newEntities.Count > 0)
            {
                foreach (var entity in newEntities)
                {
                    Logger.LogInformation("[Seeder] Preparing to seed {EntityName}: {EntityInfo}", typeof(T).Name, GetEntityInfo(entity));
                }

                await Context.Set<T>().AddRangeAsync(newEntities, ct);
            }

            if (newEntities.Count > 0 || Context.ChangeTracker.HasChanges())
            {
                await Context.SaveChangesAsync(ct);
                Logger.LogInformation("[Seeder] Successfully seeded {Count} record(s) for {EntityName}.", newEntities.Count, typeof(T).Name);
            }
            else
            {
                Logger.LogDebug("[Seeder] No new records to seed for {EntityName}.", typeof(T).Name);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Seeder] An error occurred while executing seeder for {EntityName}.", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Computes or retrieves the list of entities that need to be inserted into the database.
    /// </summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A list of new entities to insert.</returns>
    protected abstract Task<List<T>> GetNewEntitiesAsync(CancellationToken ct = default);

    /// <summary>
    /// Generates a diagnostic string representing the entity for logging.
    /// </summary>
    /// <param name="entity">The entity to inspect.</param>
    /// <returns>A string representation of the entity.</returns>
    protected abstract string GetEntityInfo(T entity);
}
