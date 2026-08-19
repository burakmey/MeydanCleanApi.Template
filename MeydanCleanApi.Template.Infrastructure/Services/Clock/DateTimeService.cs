using MeydanCleanApi.Template.Application.Abstractions.Clock;

namespace MeydanCleanApi.Template.Infrastructure.Services.Clock;

/// <summary>
/// Infrastructure implementation of <see cref="IDateTimeService"/> returning the real system clock.
/// </summary>
/// <remarks>
/// Always take the current time from this service instead of calling <c>DateTime.UtcNow</c> directly.
/// Tests can then swap in a fake clock and assert on things like token expiry without waiting.
/// </remarks>
public sealed class DateTimeService : IDateTimeService
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
