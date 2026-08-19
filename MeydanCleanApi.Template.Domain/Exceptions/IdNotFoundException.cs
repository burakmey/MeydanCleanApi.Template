namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when a domain entity cannot be found by its primary key (HTTP 404 Not Found).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// throw IdNotFoundException.For&lt;SampleProduct&gt;();
/// </code>
/// <para>
/// <strong>Resulting HTTP 404 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 404,
///   "errorCode": "ERR_ID_NOT_FOUND",
///   "message": "SampleProduct was not found.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class IdNotFoundException : AppException
{
    private IdNotFoundException(Error error) : base(error, HttpStatusCodes.NotFound) { }

    /// <summary>
    /// Creates an instance of <see cref="IdNotFoundException"/> for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The type of the domain entity.</typeparam>
    /// <returns>A new <see cref="IdNotFoundException"/> instance.</returns>
    public static IdNotFoundException For<T>() => new(new Error(ErrorCodes.IdNotFound, typeof(T).Name));
}
