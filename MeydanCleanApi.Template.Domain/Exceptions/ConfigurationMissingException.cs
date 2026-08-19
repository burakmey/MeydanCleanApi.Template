namespace MeydanCleanApi.Template.Domain.Exceptions;

/// <summary>
/// Thrown when a required configuration section or property is missing (HTTP 500 Internal Server Error).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// throw ConfigurationMissingException.ForSection("JwtOptions", "SecretKey");
/// </code>
/// <para>
/// <strong>Resulting HTTP 500 JSON Response:</strong>
/// </para>
/// <code>
/// {
///   "status": 500,
///   "errorCode": "ERR_CONFIGURATION_MISSING",
///   "message": "Required configuration section 'JwtOptions:SecretKey' is missing.",
///   "errors": null
/// }
/// </code>
/// </remarks>
public sealed class ConfigurationMissingException : AppException
{
    private ConfigurationMissingException(Error error) : base(error, HttpStatusCodes.InternalServerError) { }

    /// <summary>
    /// Creates an instance of <see cref="ConfigurationMissingException"/> for the specified section and optional property.
    /// </summary>
    /// <param name="sectionName">The name of the configuration section.</param>
    /// <param name="propertyName">Optional property name within the section.</param>
    /// <returns>A new <see cref="ConfigurationMissingException"/> instance.</returns>
    public static ConfigurationMissingException ForSection(string sectionName, string? propertyName = null)
        => new(propertyName is null
            ? new Error(ErrorCodes.ConfigurationMissing, sectionName)
            : new Error(ErrorCodes.ConfigurationMissing, sectionName, propertyName));
}
