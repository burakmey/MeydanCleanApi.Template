namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Service contract for administrative user operations (e.g. role assignment, account lockouts).
/// </summary>
public interface IUserAdminService
{
    /// <summary>
    /// Assigns a specified role to a target user account.
    /// </summary>
    Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default);

    /// <summary>
    /// Revokes a specified role from a target user account.
    /// </summary>
    Task RevokeRoleAsync(Guid userId, string roleName, CancellationToken ct = default);

    /// <summary>
    /// Toggles administrative account lockout state for the target user.
    /// </summary>
    Task SetAccountLockoutAsync(Guid userId, bool isLockedOut, CancellationToken ct = default);
}
