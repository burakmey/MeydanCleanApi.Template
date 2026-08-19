namespace MeydanCleanApi.Template.Application.Abstractions.Tracing;

/// <summary>
/// Service contract providing access to the unique request correlation identifier for distributed tracing and logging.
/// </summary>
/// <remarks>
/// The correlation ID ties together log entries, HTTP response headers, and downstream operations for a single request context.
/// </remarks>
public interface ICorrelationIdContext
{
    /// <summary>
    /// Gets the current request correlation identifier. Taken from incoming <c>X-Correlation-ID</c> header or generated if missing.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Sets the correlation identifier for the active request context (invoked by correlation middleware).
    /// </summary>
    /// <param name="correlationId">Correlation ID string.</param>
    void Set(string correlationId);
}
