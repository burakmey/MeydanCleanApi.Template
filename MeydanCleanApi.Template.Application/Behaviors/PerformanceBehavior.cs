using System.Diagnostics;
using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Tracing;
using Microsoft.Extensions.Logging;

namespace MeydanCleanApi.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior for monitoring request execution duration and flagging slow queries (>500ms).
/// </summary>
/// <typeparam name="TRequest">Incoming request type.</typeparam>
/// <typeparam name="TResponse">Outgoing response type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="PerformanceBehavior{TRequest, TResponse}"/> class.
/// </remarks>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService,
    ICorrelationIdContext correlationIdContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowRequestThresholdMilliseconds = 500;

    private readonly Stopwatch _timer = new();
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICorrelationIdContext _correlationIdContext = correlationIdContext;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > SlowRequestThresholdMilliseconds)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            var correlationId = _correlationIdContext.CorrelationId;

            _logger.LogWarning(
                "Slow Request Detected: {RequestName} ({ElapsedMilliseconds} ms) [UserId: {UserId}, CorrelationId: {CorrelationId}]",
                requestName, elapsedMilliseconds, userId, correlationId);
        }

        return response;
    }
}
