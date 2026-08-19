namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when user authentication fails due to invalid credentials (HTTP 401 Unauthorized).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// throw InvalidCredentialsException.WithCode();
/// </code>
/// <para>
/// <strong>Resulting HTTP 401 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 401,
///   "errorCode": "ERR_INVALID_CREDENTIALS",
///   "message": "Invalid email or password.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class InvalidCredentialsException : AppException
{
    private InvalidCredentialsException(Error error) : base(error, HttpStatusCodes.Unauthorized) { }

    /// <summary>
    /// Creates an instance of <see cref="InvalidCredentialsException"/> with an optional specific error code.
    /// </summary>
    /// <param name="errorCode">Optional error code. Defaults to <see cref="ErrorCodes.InvalidCredentials"/>.</param>
    /// <returns>A new <see cref="InvalidCredentialsException"/> instance.</returns>
    public static InvalidCredentialsException WithCode(string errorCode = ErrorCodes.InvalidCredentials)
        => new(new Error(errorCode));
}
