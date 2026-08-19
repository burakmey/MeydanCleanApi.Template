namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when the authenticated user lacks permission to access a resource (HTTP 403 Forbidden).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// throw ForbiddenException.WithCode();
/// </code>
/// <para>
/// <strong>Resulting HTTP 403 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 403,
///   "errorCode": "ERR_FORBIDDEN",
///   "message": "Access is forbidden.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class ForbiddenException : AppException
{
    private ForbiddenException(Error error) : base(error, HttpStatusCodes.Forbidden) { }

    /// <summary>
    /// Creates an instance of <see cref="ForbiddenException"/> with an optional specific error code.
    /// </summary>
    /// <param name="errorCode">Optional error code. Defaults to <see cref="ErrorCodes.Forbidden"/>.</param>
    /// <returns>A new <see cref="ForbiddenException"/> instance.</returns>
    public static ForbiddenException WithCode(string errorCode = ErrorCodes.Forbidden)
        => new(new Error(errorCode));
}
