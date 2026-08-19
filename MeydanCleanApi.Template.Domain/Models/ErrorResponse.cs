namespace MeydanCleanApi.Template.Domain.Models;

/// <summary>
/// Standard unified API error payload returned across all endpoints when an operation fails.
/// </summary>
/// <remarks>
/// <para>
/// <strong>JSON Response Payload Example (404 Not Found):</strong>
/// </para>
/// <code>
/// {
///   "status": 404,
///   "errorCode": "ERR_ID_NOT_FOUND",
///   "message": "SampleProduct was not found.",
///   "errors": null
/// }
/// </code>
/// <para>
/// <strong>JSON Response Payload Example (400 Bad Request with Field Errors):</strong>
/// </para>
/// <code>
/// {
///   "status": 400,
///   "errorCode": "ERR_VALIDATION",
///   "message": "Validation failed.",
///   "errors": {
///     "Email": [ "Email is required.", "Email format is invalid." ]
///   }
/// }
/// </code>
/// </remarks>
public sealed record ErrorResponse
{
    /// <summary>
    /// Gets the HTTP status code (e.g., 400, 401, 404, 500).
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Gets the stable, locale-independent error code identifier (e.g., "ERR_ID_NOT_FOUND").
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the human-readable localized error description.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets property validation errors grouped by field name, if applicable.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}
