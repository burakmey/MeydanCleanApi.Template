namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with existing system state (HTTP 409 Conflict).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// throw ConflictException.WithCode(ErrorCodes.EmailInUse);
/// </code>
/// <para>
/// <strong>Resulting HTTP 409 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 409,
///   "errorCode": "ERR_EMAIL_IN_USE",
///   "message": "Email address is already in use.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class ConflictException : AppException
{
    private ConflictException(Error error) : base(error, HttpStatusCodes.Conflict) { }

    /// <summary>
    /// Creates an instance of <see cref="ConflictException"/> with an optional specific error code.
    /// </summary>
    /// <param name="errorCode">Optional error code. Defaults to <see cref="ErrorCodes.Conflict"/>.</param>
    /// <returns>A new <see cref="ConflictException"/> instance.</returns>
    public static ConflictException WithCode(string errorCode = ErrorCodes.Conflict)
        => new(new Error(errorCode));
}
