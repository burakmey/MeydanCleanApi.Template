using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Domain.Entities.Identity;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Common.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MeydanCleanApi.Template.Infrastructure.Services.Auth;

/// <summary>
/// Infrastructure implementation of <see cref="IUserAdminService"/> for administrative user operations.
/// </summary>
/// <remarks>
/// Two rules are enforced here rather than in the calling code, so they cannot be forgotten:
/// SuperAdmin accounts cannot be modified, and only roles listed in
/// <see cref="RoleConstants.AssignableRoles"/> can be handed out. That stops an Admin from
/// promoting somebody (or themselves) to SuperAdmin.
/// </remarks>
public sealed class UserAdminService(
    UserManager<AppUser> userManager,
    IUserSessionService userSessionService,
    ILogger<UserAdminService> logger) : IUserAdminService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserSessionService _userSessionService = userSessionService;
    private readonly ILogger<UserAdminService> _logger = logger;

    /// <inheritdoc />
    public async Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        if (!RoleConstants.AssignableRoles.Contains(roleName))
        {
            throw ForbiddenException.WithCode(ErrorCodes.RoleNotAssignable);
        }

        var user = await GetProtectedUserAsync(userId);

        if (await _userManager.IsInRoleAsync(user, roleName)) return;

        var result = await _userManager.AddToRoleAsync(user, roleName);
        ThrowIfFailed(result, ErrorCodes.RoleNotAssignable);

        _logger.LogInformation("Role {Role} assigned to account {UserId}.", roleName, userId);
    }

    /// <inheritdoc />
    public async Task RevokeRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        if (!RoleConstants.AssignableRoles.Contains(roleName))
        {
            throw ForbiddenException.WithCode(ErrorCodes.RoleNotAssignable);
        }

        var user = await GetProtectedUserAsync(userId);

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        ThrowIfFailed(result, ErrorCodes.RoleNotAssignable);

        _logger.LogInformation("Role {Role} revoked from account {UserId}.", roleName, userId);
    }

    /// <inheritdoc />
    public async Task SetAccountLockoutAsync(Guid userId, bool isLockedOut, CancellationToken ct = default)
    {
        var user = await GetProtectedUserAsync(userId);

        // Lockout only takes effect when the account has the feature switched on.
        await _userManager.SetLockoutEnabledAsync(user, true);

        // A date far in the future means "locked until an administrator unlocks it".
        var lockoutEnd = isLockedOut ? DateTimeOffset.MaxValue : (DateTimeOffset?)null;
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        ThrowIfFailed(result, ErrorCodes.Conflict);

        // Locking the account also ends the current session, otherwise the user stays signed in
        // until their refresh token expires on its own.
        if (isLockedOut)
        {
            await _userSessionService.RevokeAllUserSessionsAsync(userId, ct);
        }

        _logger.LogInformation("Lockout for account {UserId} set to {IsLockedOut}.", userId, isLockedOut);
    }

    /// <summary>
    /// Loads a user and refuses to continue when the target is a SuperAdmin.
    /// </summary>
    private async Task<AppUser> GetProtectedUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw IdNotFoundException.For<AppUser>();

        if (await _userManager.IsInRoleAsync(user, RoleConstants.SuperAdmin))
        {
            throw ForbiddenException.WithCode(ErrorCodes.SuperAdminProtected);
        }

        return user;
    }

    private static void ThrowIfFailed(IdentityResult result, string errorCode)
    {
        if (result.Succeeded) return;

        throw ConflictException.WithCode(errorCode);
    }
}
