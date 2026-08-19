using MeydanCleanApi.Template.Domain.Entities.Identity;

namespace MeydanCleanApi.Template.Domain.Entities.Auths;

/// <summary>
/// Represents a link between a user account and an authentication provider.
/// </summary>
public class UserAuthProvider : BaseEntity<(Guid UserId, int AuthProviderId)>
{
    /// <summary>
    /// Gets or sets the composite key identifier for this relation.
    /// </summary>
    public override (Guid UserId, int AuthProviderId) Id
    {
        get => (UserId, AuthProviderId);
        set
        {
            UserId = value.UserId;
            AuthProviderId = value.AuthProviderId;
        }
    }

    /// <summary>
    /// Gets or sets the foreign key identifier of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the authentication provider.
    /// </summary>
    public required int AuthProviderId { get; set; }

    /// <summary>
    /// Gets or sets the unique key/subject ID issued by the provider.
    /// </summary>
    public required string ProviderKey { get; set; }

    /// <summary>
    /// Gets or sets the associated user entity.
    /// </summary>
    public AppUser? User { get; set; }

    /// <summary>
    /// Gets or sets the associated authentication provider entity.
    /// </summary>
    public AuthProvider? AuthProvider { get; set; }
}
