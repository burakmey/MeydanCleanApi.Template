namespace MeydanCleanApi.Template.Domain.Models;

/// <summary>
/// Immutable value object carrying a locale-independent error code and optional formatting arguments.
/// </summary>
/// <param name="Code">Unique error code identifier used for message localization key lookup.</param>
/// <param name="Args">Optional parameters used to format localized message templates.</param>
/// <remarks>
/// <para>
/// <strong>Code Usage Example:</strong>
/// </para>
/// <code>
/// // Simple error code without arguments
/// var error = new Error(ErrorCodes.Unauthorized);
/// 
/// // Error code with formatting parameters (e.g., entity name)
/// var entityError = new Error(ErrorCodes.IdNotFound, "SampleProduct");
/// </code>
/// </remarks>
public sealed record Error(string Code, params object[] Args);
