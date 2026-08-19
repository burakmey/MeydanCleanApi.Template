namespace MeydanCleanApi.Template.Application.Abstractions.Messaging;

/// <summary>
/// Defines a handler for a specific CQRS request of type <typeparamref name="TRequest"/> returning <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The request type implementing <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the incoming CQRS request asynchronously.
    /// </summary>
    /// <param name="request">The request payload instance.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of type <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}
