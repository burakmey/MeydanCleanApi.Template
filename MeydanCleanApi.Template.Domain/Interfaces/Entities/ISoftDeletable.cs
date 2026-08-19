namespace MeydanCleanApi.Template.Domain.Interfaces.Entities;

/// <summary>
/// Defines a contract for entities that support soft deletion.
/// </summary>
/// <remarks>
/// When an entity is soft-deleted, <see cref="IsActive"/> is set to false instead of removing the row from the database.
/// Global query filters in EF Core automatically hide inactive rows from public read queries.
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is active.
    /// Inactive entities are treated as soft-deleted for public read operations.
    /// </summary>
    bool IsActive { get; set; }
}
