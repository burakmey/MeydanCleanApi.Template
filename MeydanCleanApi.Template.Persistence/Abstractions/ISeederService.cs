namespace MeydanCleanApi.Template.Persistence.Abstractions;

/// <summary>
/// Defines a contract for database seeder services.
/// </summary>
public interface ISeederService
{
    /// <summary>
    /// Executes the database seeding logic asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SeedAsync(CancellationToken ct = default);
}

/// <summary>
/// Defines a generic contract for entity-specific database seeder services.
/// </summary>
/// <typeparam name="T">The domain entity type implementing <see cref="IBaseEntity"/>.</typeparam>
public interface ISeederService<T> : ISeederService where T : class, IBaseEntity
{
}
