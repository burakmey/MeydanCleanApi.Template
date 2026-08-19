namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Centralized, locale-independent generic validation error code constants.
/// Format: UPPERCASE string codes with underscores (e.g. "VAL_NAME_REQUIRED").
/// Generic names allow maximum reusability across all entities.
/// </summary>
public static class ValidationCodes
{
    // ── Common Entity Rules ──────────────────────────────────────────

    /// <summary>Identifier is required and must not be empty ("VAL_ID_REQUIRED").</summary>
    public const string IdRequired = "VAL_ID_REQUIRED";

    /// <summary>Search term exceeds maximum character length ("VAL_SEARCH_TERM_TOO_LONG").</summary>
    public const string SearchTermTooLong = "VAL_SEARCH_TERM_TOO_LONG";

    /// <summary>Category identifier is required ("VAL_CATEGORY_REQUIRED").</summary>
    public const string CategoryRequired = "VAL_CATEGORY_REQUIRED";

    /// <summary>Name field is required ("VAL_NAME_REQUIRED").</summary>
    public const string NameRequired = "VAL_NAME_REQUIRED";

    /// <summary>Name field exceeds maximum character length ("VAL_NAME_MAX_LENGTH").</summary>
    public const string NameMaxLength = "VAL_NAME_MAX_LENGTH";

    /// <summary>Title field is required ("VAL_TITLE_REQUIRED").</summary>
    public const string TitleRequired = "VAL_TITLE_REQUIRED";

    /// <summary>Title field exceeds maximum character length ("VAL_TITLE_MAX_LENGTH").</summary>
    public const string TitleMaxLength = "VAL_TITLE_MAX_LENGTH";

    /// <summary>Price value is invalid or less than minimum ("VAL_PRICE_INVALID").</summary>
    public const string PriceInvalid = "VAL_PRICE_INVALID";

    // ── Pagination ───────────────────────────────────────────────────

    /// <summary>Page number is less than 1 ("VAL_PAGE_NUMBER_INVALID").</summary>
    public const string PageNumberInvalid = "VAL_PAGE_NUMBER_INVALID";

    /// <summary>Page size is outside allowed limits ("VAL_PAGE_SIZE_INVALID").</summary>
    public const string PageSizeInvalid = "VAL_PAGE_SIZE_INVALID";

    // ── File Upload ──────────────────────────────────────────────────

    /// <summary>Original file name is required ("VAL_FILE_NAME_REQUIRED").</summary>
    public const string FileNameRequired = "VAL_FILE_NAME_REQUIRED";

    /// <summary>File size is zero or invalid ("VAL_FILE_SIZE_INVALID").</summary>
    public const string FileSizeInvalid = "VAL_FILE_SIZE_INVALID";

    /// <summary>File size exceeds the allowed maximum ("VAL_FILE_SIZE_TOO_LARGE").</summary>
    public const string FileSizeTooLarge = "VAL_FILE_SIZE_TOO_LARGE";

    /// <summary>Storage container is not on the allowed list ("VAL_FILE_CONTAINER_INVALID").</summary>
    public const string FileContainerInvalid = "VAL_FILE_CONTAINER_INVALID";

    /// <summary>Sub-folder name contains characters that are not allowed ("VAL_FILE_SUBFOLDER_INVALID").</summary>
    public const string FileSubFolderInvalid = "VAL_FILE_SUBFOLDER_INVALID";

    /// <summary>File extension is not on the allowed list ("VAL_FILE_EXTENSION_INVALID").</summary>
    public const string FileExtensionInvalid = "VAL_FILE_EXTENSION_INVALID";

    /// <summary>MIME content type is required ("VAL_FILE_CONTENT_TYPE_REQUIRED").</summary>
    public const string FileContentTypeRequired = "VAL_FILE_CONTENT_TYPE_REQUIRED";

    /// <summary>File purpose is not a known value ("VAL_FILE_PURPOSE_INVALID").</summary>
    public const string FilePurposeInvalid = "VAL_FILE_PURPOSE_INVALID";

    /// <summary>Sort order must not be negative ("VAL_SORT_ORDER_INVALID").</summary>
    public const string SortOrderInvalid = "VAL_SORT_ORDER_INVALID";

    // ── Auth ─────────────────────────────────────────────────────────

    /// <summary>Email address is required ("VAL_EMAIL_REQUIRED").</summary>
    public const string EmailRequired = "VAL_EMAIL_REQUIRED";

    /// <summary>Email address format is invalid ("VAL_EMAIL_INVALID").</summary>
    public const string EmailInvalid = "VAL_EMAIL_INVALID";

    /// <summary>Password is required ("VAL_PASSWORD_REQUIRED").</summary>
    public const string PasswordRequired = "VAL_PASSWORD_REQUIRED";
}
