using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Tracing;
using MeydanCleanApi.Template.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MeydanCleanApi.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatically logging CQRS request execution, duration, and correlation context.
/// </summary>
/// <typeparam name="TRequest">Incoming request type.</typeparam>
/// <typeparam name="TResponse">Outgoing response type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
/// </remarks>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICorrelationIdContext correlationIdContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ICorrelationIdContext _correlationIdContext = correlationIdContext;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = _correlationIdContext.CorrelationId;

        _logger.LogInformation("Processing CQRS Request {RequestName} [CorrelationId: {CorrelationId}]", requestName, correlationId);

        try
        {
            var response = await next();
            _logger.LogInformation("Successfully Processed CQRS Request {RequestName} [CorrelationId: {CorrelationId}]", requestName, correlationId);
            return response;
        }
        catch (AppException ex)
        {
            // Expected business outcomes: a wrong password, a missing id, a validation failure. Logging
            // these as errors would fill the error log with normal traffic and make real faults harder
            // to spot, so they stay at Warning and the stack trace is left out.
            _logger.LogWarning(
                "Rejected CQRS Request {RequestName}: {ErrorCode} [CorrelationId: {CorrelationId}]",
                requestName, ex.Error.Code, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            // Anything else is a genuine fault, so keep it at Error with the full exception.
            _logger.LogError(ex, "Failed Processing CQRS Request {RequestName} [CorrelationId: {CorrelationId}]", requestName, correlationId);
            throw;
        }
    }
}
