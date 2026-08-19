namespace MeydanCleanApi.Template.Application.Abstractions.Options;

/// <summary>
/// Contract interface enforcing strongly-typed configuration option models to declare their <c>appsettings.json</c> section name.
/// </summary>
public interface IOptionSection
{
    /// <summary>
    /// Gets the configuration section name in <c>appsettings.json</c>.
    /// </summary>
    static abstract string SectionName { get; }
}
