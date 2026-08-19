namespace MeydanCleanApi.Template.Domain.Exceptions.Base;

/// <summary>
/// Abstract base class for all domain-specific application exceptions.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Design Pattern:</strong> Subclasses map domain errors to specific HTTP status codes.
/// Subclasses use static factory methods (e.g., <c>IdNotFoundException.For&lt;T&gt;()</c>) to ensure consistent instantiation.
/// </para>
/// <para>
/// <strong>Usage Example:</strong>
/// </para>
/// <code>
/// // Throwing a domain exception inside a Command/Query Handler:
/// var product = await _repository.GetByIdAsync(id, cancellationToken);
/// if (product is null)
/// {
///     throw IdNotFoundException.For&lt;SampleProduct&gt;();
/// }
/// </code>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="AppException"/> class.
/// </remarks>
/// <param name="error">Localizable error value object.</param>
/// <param name="statusCode">HTTP status code corresponding to the error.</param>
public abstract class AppException(Error error, int statusCode) : Exception(error.Code)
{

    /// <summary>
    /// Gets the localized error detail model.
    /// </summary>
    public Error Error { get; } = error;

    /// <summary>
    /// Gets the HTTP status code representing the error.
    /// </summary>
    public int StatusCode { get; } = statusCode;
}
