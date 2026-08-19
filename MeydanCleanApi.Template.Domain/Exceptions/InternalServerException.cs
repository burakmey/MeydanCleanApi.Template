namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when an unexpected internal server error occurs (HTTP 500 Internal Server Error).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// throw InternalServerException.WithCode();
/// </code>
/// <para>
/// <strong>Resulting HTTP 500 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 500,
///   "errorCode": "ERR_INTERNAL",
///   "message": "An internal server error occurred.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class InternalServerException : AppException
{
    private InternalServerException(Error error) : base(error, HttpStatusCodes.InternalServerError) { }

    /// <summary>
    /// Creates an instance of <see cref="InternalServerException"/> with an optional specific error code.
    /// </summary>
    /// <param name="errorCode">Optional error code. Defaults to <see cref="ErrorCodes.InternalServer"/>.</param>
    /// <param name="args">Optional values substituted into the localized message, such as an entity name.</param>
    /// <returns>A new <see cref="InternalServerException"/> instance.</returns>
    public static InternalServerException WithCode(string errorCode = ErrorCodes.InternalServer, params object[] args)
        => new(new Error(errorCode, args));
}
