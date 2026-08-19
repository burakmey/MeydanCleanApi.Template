namespace MeydanCleanApi.Template.Application.Constants.FileStorage;

/// <summary>
/// Defines storage folder root containers for public, private, and temporary file organization.
/// </summary>
public static class FileStorageContainers
{
    /// <summary>
    /// Containers whose files are publicly accessible via direct URL.
    /// </summary>
    public static class Public
    {
        /// <summary>Product image gallery container. Sub-folder: product id.</summary>
        public const string Products = "public/products";

        /// <summary>Category image container. Sub-folder: category id.</summary>
        public const string ProductCategories = "public/product-categories";

        /// <summary>User avatar image container. Sub-folder: user id.</summary>
        public const string Users = "public/users";
    }

    /// <summary>
    /// Containers whose files require time-limited presigned URL access.
    /// </summary>
    public static class Private
    {
        /// <summary>User private documents container. Sub-folder: user id.</summary>
        public const string UserDocuments = "private/users/documents";

        /// <summary>System private documents container.</summary>
        public const string Documents = "private/documents";
    }

    /// <summary>
    /// Temporary scratch space container for unconfirmed uploads.
    /// </summary>
    public static class Temp
    {
        /// <summary>Default temp container.</summary>
        public const string Default = "temp";
    }

    /// <summary>
    /// Whitelist array of every allowed container root.
    /// </summary>
    public static readonly string[] All =
    [
        Public.Products,
        Public.ProductCategories,
        Public.Users,
        Private.UserDocuments,
        Private.Documents,
        Temp.Default,
    ];

    /// <summary>Prefix marking containers whose files are readable without a signed URL.</summary>
    public const string PublicPrefix = "public/";

    /// <summary>
    /// Returns whether files in <paramref name="container"/> are publicly accessible without a signed URL.
    /// </summary>
    public static bool IsPublic(string container)
        => !string.IsNullOrEmpty(container) && container.StartsWith(PublicPrefix, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether <paramref name="container"/> is one of the containers listed in <see cref="All"/>.
    /// </summary>
    /// <param name="container">Container name supplied by the caller.</param>
    /// <returns><c>true</c> when the container is on the whitelist.</returns>
    /// <remarks>
    /// Always check this before using a container name to build a storage path. The value arrives from
    /// the client, and an unchecked value like <c>../../</c> or <c>C:/Windows</c> would let a caller
    /// read or delete files outside the storage folder.
    /// </remarks>
    public static bool IsAllowed(string? container)
        => !string.IsNullOrWhiteSpace(container) && All.Contains(container, StringComparer.Ordinal);
}
