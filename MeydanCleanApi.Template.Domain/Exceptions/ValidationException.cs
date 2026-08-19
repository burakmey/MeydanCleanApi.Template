namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when request input validation rules fail (HTTP 400 Bad Request).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// var errors = new Dictionary&lt;string, string[]&gt;
/// {
///     { "Email", new[] { "Email is required.", "Email format is invalid." } }
/// };
/// throw ValidationException.From(errors);
/// </code>
/// <para>
/// <strong>Resulting HTTP 400 JSON Response:</strong>
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
public sealed class ValidationException : AppException
{
    /// <summary>
    /// Gets property validation errors grouped by field name.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    private ValidationException(IDictionary<string, string[]> errors)
        : base(new Error(ErrorCodes.Validation), HttpStatusCodes.BadRequest)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a new <see cref="ValidationException"/> instance from a dictionary of property errors.
    /// </summary>
    /// <param name="errors">Dictionary of validation errors.</param>
    /// <returns>A new <see cref="ValidationException"/> instance.</returns>
    public static ValidationException From(IDictionary<string, string[]> errors) => new(errors);
}
