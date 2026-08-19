namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when authentication is required but missing or invalid (HTTP 401 Unauthorized).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// // Generic 401 Unauthorized
/// throw UnauthorizedException.WithCode();
/// 
/// // Custom 401 Unauthorized with specific error code
/// throw UnauthorizedException.WithCode(ErrorCodes.InvalidCredentials);
/// </code>
/// <para>
/// <strong>Resulting HTTP 401 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 401,
///   "errorCode": "ERR_UNAUTHORIZED",
///   "message": "Authentication is required.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class UnauthorizedException : AppException
{
    private UnauthorizedException(Error error) : base(error, HttpStatusCodes.Unauthorized) { }

    /// <summary>
    /// Creates an instance of <see cref="UnauthorizedException"/> with an optional specific error code.
    /// </summary>
    /// <param name="errorCode">Optional error code. Defaults to <see cref="ErrorCodes.Unauthorized"/>.</param>
    /// <returns>A new <see cref="UnauthorizedException"/> instance.</returns>
    public static UnauthorizedException WithCode(string errorCode = ErrorCodes.Unauthorized)
        => new(new Error(errorCode));
}
