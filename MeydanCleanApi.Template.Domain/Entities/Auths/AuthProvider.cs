namespace MeydanCleanApi.Template.Domain.Entities.Auths;

/// <summary>
/// Represents an external or local authentication provider (e.g., Local, Google, Apple).
/// </summary>
public class AuthProvider : BasicEntity
{
    /// <summary>
    /// Gets or sets the collection of user accounts linked to this provider.
    /// </summary>
    public ICollection<UserAuthProvider> UserAuthProviders { get; set; } = [];
}
