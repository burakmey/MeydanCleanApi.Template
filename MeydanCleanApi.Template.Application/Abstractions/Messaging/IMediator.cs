namespace MeydanCleanApi.Template.Application.Abstractions.Messaging;

/// <summary>
/// In-process mediator contract for dispatching CQRS Commands and Queries through the pipeline chain to their handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a CQRS request through the registered pipeline behaviors to its target handler.
    /// </summary>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    /// <param name="request">CQRS request payload instance.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of type <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
