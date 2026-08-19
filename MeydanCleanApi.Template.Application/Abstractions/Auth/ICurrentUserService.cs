namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Service contract providing type-safe access to the authenticated user identity for the active HTTP request context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the authenticated user, or <c>null</c> if unauthenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the email address of the authenticated user, or <c>null</c> if unauthenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the list of role names assigned to the authenticated user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets a value indicating whether the current request is executing under an authenticated user principal.
    /// </summary>
    bool IsAuthenticated { get; }
}
