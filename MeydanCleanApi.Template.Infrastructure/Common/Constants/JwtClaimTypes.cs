namespace MeydanCleanApi.Template.Infrastructure.Common.Constants;

/// <summary>
/// Standardized JWT claim string constants used across token validation and verification.
/// </summary>
public static class JwtClaimTypes
{
    /// <summary>Subject identifier (user unique ID or OAuth sub).</summary>
    public const string Subject = "sub";

    /// <summary>Email address claim.</summary>
    public const string Email = "email";

    /// <summary>Full name claim.</summary>
    public const string Name = "name";

    /// <summary>Email verified boolean claim.</summary>
    public const string EmailVerified = "email_verified";

    /// <summary>JWT ID claim for single session tracking.</summary>
    public const string Jti = "jti";

    /// <summary>Role claim.</summary>
    public const string Role = "role";
}
