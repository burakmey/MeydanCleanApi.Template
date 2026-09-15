using MeydanCleanApi.Template.Application.Abstractions.Clock;

namespace MeydanCleanApi.Template.Tests.Common;

/// <summary>
/// Clock that stays where it is put, so a test can assert on an expiry without waiting for one.
/// </summary>
internal sealed class FixedClock(DateTime utcNow) : IDateTimeService
{
    public DateTime UtcNow { get; private set; } = utcNow;

    /// <summary>Moves the clock forward, to walk past an expiry the code under test wrote earlier.</summary>
    public void Advance(TimeSpan amount) => UtcNow = UtcNow.Add(amount);
}
