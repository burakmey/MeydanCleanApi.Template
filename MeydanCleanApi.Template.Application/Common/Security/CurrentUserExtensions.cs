using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Exceptions;

namespace MeydanCleanApi.Template.Application.Common.Security;

/// <summary>
/// Ownership checks shared by handlers that work on records belonging to a single user.
/// </summary>
/// <remarks>
/// Being signed in is not the same as being allowed. Every handler that loads a record by id must
/// also check that the record belongs to the caller, otherwise anyone can read or change another
/// user's data just by guessing ids.
/// </remarks>
public static class CurrentUserExtensions
{
    /// <summary>
    /// Returns whether the caller holds an administrative role.
    /// </summary>
    public static bool IsAdmin(this ICurrentUserService currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        return currentUser.Roles.Any(RoleConstants.AdminPanelRoles.Contains);
    }

    /// <summary>
    /// Throws unless the caller owns the record or holds an administrative role.
    /// </summary>
    /// <param name="currentUser">The signed-in user.</param>
    /// <param name="ownerUserId">
    /// Owner recorded on the entity. <c>null</c> means the record belongs to the site rather than to a
    /// person — a hero image or a public brochure, for example — and only administrators may change it.
    /// </param>
    /// <exception cref="UnauthorizedException">Thrown when nobody is signed in.</exception>
    /// <exception cref="ForbiddenException">Thrown when the caller is signed in but not allowed.</exception>
    /// <remarks>
    /// This governs <b>managing</b> a record, not reading its contents. A file in a public container is
    /// downloadable by anyone holding its URL regardless of who owns the row.
    /// </remarks>
    public static void EnsureCanAccess(this ICurrentUserService currentUser, Guid? ownerUserId)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        var userId = currentUser.UserId ?? throw UnauthorizedException.WithCode();

        if (currentUser.IsAdmin()) return;

        if (ownerUserId is null || ownerUserId != userId)
        {
            throw ForbiddenException.WithCode();
        }
    }
}
