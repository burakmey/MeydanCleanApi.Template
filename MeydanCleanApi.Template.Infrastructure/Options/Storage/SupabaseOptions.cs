namespace MeydanCleanApi.Template.Infrastructure.Options.Storage;

/// <summary>
/// Strongly-typed configuration options model for Supabase Cloud Storage.
/// </summary>
public sealed record SupabaseOptions : IOptionSection
{
    /// <inheritdoc />
    public static string SectionName => "Storage:Supabase";

    /// <summary>Gets or init-sets the Supabase project URL (e.g. "https://xyz.supabase.co").</summary>
    public required string Url { get; init; }

    /// <summary>Gets or init-sets the Supabase secret API key.</summary>
    public required string ApiKey { get; init; }

    /// <summary>Gets or init-sets the target storage bucket name.</summary>
    public required string Bucket { get; init; }

    /// <summary>Gets or init-sets the presigned download URL expiry time in minutes.</summary>
    public required int DownloadUrlExpiryMinute { get; init; }

    /// <summary>Gets or init-sets the optional webhook secret for validating Supabase storage events.</summary>
    public string? WebhookSecret { get; init; }

    /// <summary>
    /// Placeholder fragments shipped in checked-in configuration templates.
    /// </summary>
    private static readonly string[] PlaceholderMarkers =
        ["your-dev-", "your-prod-", "your-supabase", "changeme", "example.com"];

    /// <summary>
    /// Evaluates whether the options hold real credentials rather than unconfigured template placeholders.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Url) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(Bucket) &&
        !PlaceholderMarkers.Any(marker => Url.Contains(marker, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks whether the API key supplied appears to be a browser-level public key instead of a secret service key.
    /// </summary>
    public bool IsPubliclyScopedKey => LooksPublishable(ApiKey) || LooksAnonJwt(ApiKey);

    /// <summary>
    /// Returns the public HTTP URL root for accessing public bucket objects.
    /// </summary>
    public string PublicBaseUrl => $"{Url.TrimEnd('/')}/storage/v1/object/public/{Bucket}";

    private static bool LooksPublishable(string? key) =>
        key?.StartsWith("sb_publishable_", StringComparison.OrdinalIgnoreCase) == true;

    private static bool LooksAnonJwt(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        var segments = key.Split('.');
        if (segments.Length != 3) return false;

        try
        {
            var payload = segments[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            return json.Contains("\"role\":\"anon\"", StringComparison.OrdinalIgnoreCase) ||
                   json.Contains("\"role\": \"anon\"", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
