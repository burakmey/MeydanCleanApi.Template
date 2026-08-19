using Microsoft.Extensions.Logging;

namespace MeydanCleanApi.Template.Infrastructure.Common.Logging;

/// <summary>
/// Centralized strongly-typed <see cref="EventId"/> definitions for structured logging across infrastructure.
/// </summary>
public static class LogEvents
{
    /// <summary>Authentication failure or invalid credentials log event.</summary>
    public static readonly EventId AuthFailed = new(1001, nameof(AuthFailed));

    /// <summary>Token generation log event.</summary>
    public static readonly EventId TokenIssued = new(1002, nameof(TokenIssued));

    /// <summary>External OAuth provider token verification success event.</summary>
    public static readonly EventId ExternalAuthVerified = new(1003, nameof(ExternalAuthVerified));

    /// <summary>File storage operation completed log event.</summary>
    public static readonly EventId FileStorageOperation = new(2001, nameof(FileStorageOperation));

    /// <summary>Localization missing key fallback log event.</summary>
    public static readonly EventId LocalizationKeyNotFound = new(3001, nameof(LocalizationKeyNotFound));
}
