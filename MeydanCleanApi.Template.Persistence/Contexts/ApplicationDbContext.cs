using System.Linq.Expressions;
using System.Reflection;
using MeydanCleanApi.Template.Application.Abstractions.Clock;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MeydanCleanApi.Template.Persistence.Contexts;

/// <summary>
/// The application's main Entity Framework Core DbContext, extending IdentityDbContext for unified transaction management.
/// </summary>
/// <remarks>
/// Declared <c>partial</c>: table declarations live in <c>ApplicationDbContext.Tables.cs</c> to keep business logic and schema declarations separated.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with injected options.
/// </remarks>
/// <param name="options">The DbContext options configured via Dependency Injection.</param>
/// <param name="dateTimeService">Clock used to stamp audit timestamps.</param>
public partial class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeService dateTimeService) : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    private readonly IDateTimeService _dateTimeService = dateTimeService;

    /// <summary>
    /// Saves all changes made in this context to the database, automatically stamping audit timestamps on <see cref="IBaseEntity"/> instances.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Whether the change tracker is reset once the save succeeds.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <remarks>
    /// This overload is the one every other save funnels into, including the parameterless
    /// <c>SaveChangesAsync(CancellationToken)</c> and EF Core's own internal calls. Overriding the
    /// shorter one instead would leave those paths writing rows with no timestamps at all.
    /// </remarks>
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Synchronous counterpart of <see cref="SaveChangesAsync(bool, CancellationToken)"/>.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Whether the change tracker is reset once the save succeeds.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <remarks>
    /// Nothing in this template saves synchronously, but seeders, tooling and future code can. Without
    /// this override such a call would silently skip the timestamps the async path applies.
    /// </remarks>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Stamps <see cref="IBaseEntity.CreatedAt"/> and <see cref="IBaseEntity.UpdatedAt"/> on pending changes.
    /// </summary>
    /// <remarks>
    /// The time comes from <see cref="IDateTimeService"/> rather than <c>DateTime.UtcNow</c>, so a test
    /// can pin the clock and assert on what was written.
    /// </remarks>
    private void ApplyAuditTimestamps()
    {
        var now = _dateTimeService.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IBaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    // Left out of the UPDATE entirely, so a caller cannot rewrite when a row was created.
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }

    /// <summary>
    /// Configures the EF Core model by applying entity configurations, ignoring unused Identity tables, and setting up global query filters.
    /// </summary>
    /// <param name="builder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Ignore<IdentityUserToken<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();

        base.OnModelCreating(builder);

        // Scan current assembly for all IEntityTypeConfiguration<T> classes
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply Global Query Filter for ISoftDeletable entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "x");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsActive));
                var filter = Expression.Lambda(property, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
