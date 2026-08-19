namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Defines application roles used across Identity, authorization policies, and database seeding.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Customization Guide:</strong>
/// Developers can add new application roles (e.g., <c>Manager</c>, <c>Editor</c>) here and update <see cref="AssignableRoles"/>.
/// </para>
/// <code>
/// public const string Manager = "Manager";
/// </code>
/// </remarks>
public static class RoleConstants
{
    /// <summary>Super administrator role with full system access.</summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>Administrator role for daily management operations.</summary>
    public const string Admin = "Admin";

    /// <summary>End-user customer role.</summary>
    public const string Customer = "Customer";

    /// <summary>Roles authorized to access administrative endpoints.</summary>
    public static readonly IReadOnlySet<string> AdminPanelRoles =
        new HashSet<string>(StringComparer.Ordinal) { SuperAdmin, Admin };

    /// <summary>Roles that can be assigned through administrative operations.</summary>
    public static readonly IReadOnlySet<string> AssignableRoles =
        new HashSet<string>(StringComparer.Ordinal) { Admin, Customer };
}
