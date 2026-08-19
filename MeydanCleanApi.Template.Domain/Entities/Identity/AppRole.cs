using Microsoft.AspNetCore.Identity;

namespace MeydanCleanApi.Template.Domain.Entities.Identity;

/// <summary>
/// Represents an application role extending <see cref="IdentityRole{TKey}"/> with <see cref="Guid"/> keys.
/// </summary>
public class AppRole : IdentityRole<Guid>
{
    /// <summary>
    /// Gets or sets the human-readable description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this role is an immutable system role required by the core application.
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Gets or sets the UTC timestamp when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
