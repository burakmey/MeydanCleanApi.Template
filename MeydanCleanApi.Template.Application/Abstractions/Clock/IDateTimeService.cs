namespace MeydanCleanApi.Template.Application.Abstractions.Clock;

/// <summary>
/// Service contract providing access to system time for testability.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current system timestamp in UTC.
    /// </summary>
    DateTime UtcNow { get; }
}
