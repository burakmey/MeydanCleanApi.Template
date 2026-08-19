namespace MeydanCleanApi.Template.Domain.Enums;

/// <summary>
/// Specifies the type of authentication provider (local email/password or external OAuth provider).
/// </summary>
public enum AuthProviderType
{
    /// <summary>
    /// Local user account authenticated with email and password.
    /// </summary>
    Local = 1,

    /// <summary>
    /// Google OAuth authentication provider.
    /// </summary>
    Google = 2,

    /// <summary>
    /// Apple OAuth authentication provider.
    /// </summary>
    Apple = 3
}
