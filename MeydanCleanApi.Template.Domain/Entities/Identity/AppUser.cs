using Microsoft.AspNetCore.Identity;
using MeydanCleanApi.Template.Domain.Entities.Auths;

namespace MeydanCleanApi.Template.Domain.Entities.Identity;

/// <summary>
/// Represents an application user, extending <see cref="IdentityUser{TKey}"/> with <see cref="Guid"/> keys and token management properties.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets the foreign key identifier of the most recently used authentication provider.
    /// </summary>
    public required int ActiveAuthProviderId { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public override string? Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the SHA-256 hash of the current refresh token used for session security.
    /// </summary>
    public string? RefreshTokenHash { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiration date and time of the current refresh token.
    /// </summary>
    public DateTime? RefreshTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the JWT ID (jti) of the active session token.
    /// </summary>
    public string? CurrentJti { get; set; }

    /// <summary>
    /// Gets or sets the collection of linked authentication providers for this user.
    /// </summary>
    public ICollection<UserAuthProvider> UserAuthProviders { get; set; } = [];
}
