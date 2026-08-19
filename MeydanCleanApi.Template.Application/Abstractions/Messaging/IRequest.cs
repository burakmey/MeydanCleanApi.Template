namespace MeydanCleanApi.Template.Application.Abstractions.Messaging;

/// <summary>
/// Marker contract representing a CQRS Command or Query request that returns a response payload of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response payload type returned by the request handler.</typeparam>
public interface IRequest<out TResponse>
{
}
