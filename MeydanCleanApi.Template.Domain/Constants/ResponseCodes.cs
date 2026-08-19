namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Centralized, locale-independent success response code constants.
/// Format: UPPERCASE string codes with underscores (e.g. "MSG_SUCCESS_CREATED").
/// Provides generic reusable CRUD status codes compatible with parameterized format arguments ({0}).
/// </summary>
public static class ResponseCodes
{
    /// <summary>Generic operation successful ("MSG_SUCCESS"). Used for non-CRUD workflow actions.</summary>
    public const string Success = "MSG_SUCCESS";

    /// <summary>Entity successfully created ("MSG_SUCCESS_CREATED"). Format template: "{0} created successfully."</summary>
    public const string Created = "MSG_SUCCESS_CREATED";

    /// <summary>Entity successfully updated ("MSG_SUCCESS_UPDATED"). Format template: "{0} updated successfully."</summary>
    public const string Updated = "MSG_SUCCESS_UPDATED";

    /// <summary>Entity successfully soft-deleted ("MSG_SUCCESS_SOFT_DELETED"). Format template: "{0} deleted successfully."</summary>
    public const string SoftDeleted = "MSG_SUCCESS_SOFT_DELETED";

    /// <summary>Entity permanently hard-deleted from database ("MSG_SUCCESS_HARD_DELETED"). Format template: "{0} permanently deleted."</summary>
    public const string HardDeleted = "MSG_SUCCESS_HARD_DELETED";

    /// <summary>Data successfully retrieved ("MSG_SUCCESS_FETCHED"). Format template: "{0} fetched successfully."</summary>
    public const string Fetched = "MSG_SUCCESS_FETCHED";
}
