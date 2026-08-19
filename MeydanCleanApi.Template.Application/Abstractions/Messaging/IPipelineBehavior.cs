namespace MeydanCleanApi.Template.Application.Abstractions.Messaging;

/// <summary>
/// Pipeline behavior contract for intercepting CQRS requests before and after handler execution.
/// Used for Logging, Validation, and Performance profiling behaviors.
/// </summary>
/// <typeparam name="TRequest">Incoming request type.</typeparam>
/// <typeparam name="TResponse">Outgoing response type.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Intercepts the request execution pipeline.
    /// </summary>
    /// <param name="request">Incoming request payload.</param>
    /// <param name="next">Delegate to invoke the next pipeline behavior or final handler.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Response payload of type <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct);
}
