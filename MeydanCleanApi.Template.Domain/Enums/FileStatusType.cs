namespace MeydanCleanApi.Template.Domain.Enums;

/// <summary>
/// Specifies the current upload status of a file record.
/// </summary>
public enum FileStatusType
{
    /// <summary>
    /// File upload record is created and waiting for client upload confirmation.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// File has been successfully uploaded to cloud storage.
    /// </summary>
    Uploaded = 2,

    /// <summary>
    /// File upload process failed or timed out.
    /// </summary>
    Failed = 3
}
