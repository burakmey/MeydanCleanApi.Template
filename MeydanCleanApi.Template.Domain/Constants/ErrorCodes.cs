namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Stable, locale-independent error codes used for API error responses and localized message lookups.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Generic internal server error code ("ERR_INTERNAL").</summary>
    public const string InternalServer = "ERR_INTERNAL";

    /// <summary>Entity not found by primary key ID ("ERR_ID_NOT_FOUND").</summary>
    public const string IdNotFound = "ERR_ID_NOT_FOUND";

    /// <summary>Input validation failure ("ERR_VALIDATION").</summary>
    public const string Validation = "ERR_VALIDATION";

    /// <summary>Unauthenticated access error ("ERR_UNAUTHORIZED").</summary>
    public const string Unauthorized = "ERR_UNAUTHORIZED";

    /// <summary>Access forbidden error ("ERR_FORBIDDEN").</summary>
    public const string Forbidden = "ERR_FORBIDDEN";

    /// <summary>State conflict error ("ERR_CONFLICT").</summary>
    public const string Conflict = "ERR_CONFLICT";

    /// <summary>Invalid user credentials ("ERR_INVALID_CREDENTIALS").</summary>
    public const string InvalidCredentials = "ERR_INVALID_CREDENTIALS";

    /// <summary>Refresh token is unknown, already used or expired ("ERR_SESSION_EXPIRED").</summary>
    /// <remarks>
    /// Kept separate from <see cref="InvalidCredentials"/> so a client can tell the two apart: this one
    /// means "sign in again", while invalid credentials means the email or password was wrong.
    /// </remarks>
    public const string SessionExpired = "ERR_SESSION_EXPIRED";

    /// <summary>Client sent too many requests in the current window ("ERR_TOO_MANY_REQUESTS").</summary>
    public const string TooManyRequests = "ERR_TOO_MANY_REQUESTS";

    /// <summary>File is not uploaded to storage ("ERR_FILE_NOT_UPLOADED").</summary>
    public const string FileNotUploaded = "ERR_FILE_NOT_UPLOADED";

    /// <summary>SuperAdmin account protection error ("ERR_SUPER_ADMIN_PROTECTED").</summary>
    public const string SuperAdminProtected = "ERR_SUPER_ADMIN_PROTECTED";

    /// <summary>Role is not assignable ("ERR_ROLE_NOT_ASSIGNABLE").</summary>
    public const string RoleNotAssignable = "ERR_ROLE_NOT_ASSIGNABLE";

    /// <summary>Email is already in use ("ERR_EMAIL_IN_USE").</summary>
    public const string EmailInUse = "ERR_EMAIL_IN_USE";

    /// <summary>Required configuration is missing ("ERR_CONFIGURATION_MISSING").</summary>
    public const string ConfigurationMissing = "ERR_CONFIGURATION_MISSING";

    /// <summary>Configuration value is invalid ("ERR_CONFIGURATION_INVALID").</summary>
    public const string ConfigurationInvalid = "ERR_CONFIGURATION_INVALID";

    /// <summary>File is currently in use and cannot be deleted ("ERR_FILE_IN_USE").</summary>
    public const string FileInUse = "ERR_FILE_IN_USE";

    /// <summary>Entity does not support soft delete ("ERR_SOFT_DELETE_NOT_SUPPORTED").</summary>
    public const string SoftDeleteNotSupported = "ERR_SOFT_DELETE_NOT_SUPPORTED";

    /// <summary>External authentication provider is not supported ("ERR_EXTERNAL_AUTH_PROVIDER_NOT_SUPPORTED").</summary>
    public const string ExternalAuthProviderNotSupported = "ERR_EXTERNAL_AUTH_PROVIDER_NOT_SUPPORTED";

    /// <summary>External authentication service is unavailable ("ERR_EXTERNAL_AUTH_SERVICE_UNAVAILABLE").</summary>
    public const string ExternalAuthServiceUnavailable = "ERR_EXTERNAL_AUTH_SERVICE_UNAVAILABLE";

    /// <summary>External authentication token is required ("ERR_EXTERNAL_AUTH_TOKEN_REQUIRED").</summary>
    public const string ExternalAuthTokenRequired = "ERR_EXTERNAL_AUTH_TOKEN_REQUIRED";

    /// <summary>External authentication token expired ("ERR_EXTERNAL_AUTH_TOKEN_EXPIRED").</summary>
    public const string ExternalAuthTokenExpired = "ERR_EXTERNAL_AUTH_TOKEN_EXPIRED";

    /// <summary>External authentication token invalid ("ERR_EXTERNAL_AUTH_TOKEN_INVALID").</summary>
    public const string ExternalAuthTokenInvalid = "ERR_EXTERNAL_AUTH_TOKEN_INVALID";

    /// <summary>External token missing required claims ("ERR_EXTERNAL_AUTH_MISSING_CLAIMS").</summary>
    public const string ExternalAuthMissingClaims = "ERR_EXTERNAL_AUTH_MISSING_CLAIMS";
}
