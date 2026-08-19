using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Common.Models.Auth;

/// <summary>
/// Represents verified user profile payload returned from an external OAuth provider.
/// </summary>
public sealed record ExternalUserModel
{
    /// <summary>
    /// Gets or sets the authentication provider type.
    /// </summary>
    public required AuthProviderType AuthProvider { get; init; }

    /// <summary>
    /// Gets or sets the provider's permanent user identifier (OAuth <c>sub</c> claim).
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Gets or sets the email address reported by the external provider.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the provider verified the email address.
    /// </summary>
    public required bool EmailVerified { get; init; }

    /// <summary>
    /// Gets or sets the user's display name reported by the provider.
    /// </summary>
    public required string Name { get; init; }
}
