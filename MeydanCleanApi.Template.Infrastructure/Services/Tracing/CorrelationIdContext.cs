using MeydanCleanApi.Template.Application.Abstractions.Tracing;

namespace MeydanCleanApi.Template.Infrastructure.Services.Tracing;

/// <summary>
/// Infrastructure scoped implementation of <see cref="ICorrelationIdContext"/> for tracking HTTP request correlation IDs.
/// </summary>
public sealed class CorrelationIdContext : ICorrelationIdContext
{
    private string _correlationId = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    public string CorrelationId => _correlationId;

    /// <inheritdoc />
    public void Set(string correlationId)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            _correlationId = correlationId;
        }
    }
}
