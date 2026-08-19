namespace MeydanCleanApi.Template.Domain.Enums;

/// <summary>
/// Specifies the target cloud or local storage service provider.
/// </summary>
public enum FileStorageType
{
    /// <summary>
    /// Local disk storage provider (default for development environments).
    /// </summary>
    Local = 1,

    /// <summary>
    /// Supabase cloud storage service provider.
    /// </summary>
    Supabase = 2,

    /// <summary>
    /// Amazon Web Services (AWS) S3 storage provider.
    /// </summary>
    AWS = 3,

    /// <summary>
    /// Google Cloud Storage provider.
    /// </summary>
    Google = 4,

    /// <summary>
    /// Microsoft Azure Blob Storage provider.
    /// </summary>
    Azure = 5
}
