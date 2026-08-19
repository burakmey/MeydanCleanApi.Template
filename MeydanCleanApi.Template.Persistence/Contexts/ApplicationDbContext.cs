using System.Linq.Expressions;
using System.Reflection;
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
public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser, AppRole, Guid>(options)
{

    /// <summary>
    /// Saves all changes made in this context to the database, automatically stamping audit timestamps on <see cref="IBaseEntity"/> instances.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IBaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
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
