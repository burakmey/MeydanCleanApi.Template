namespace MeydanCleanApi.Template.Application.Abstractions.Messaging;

/// <summary>
/// Continuation delegate for invoking the next pipeline behavior or the final request handler.
/// </summary>
/// <typeparam name="TResponse">The response payload type.</typeparam>
/// <returns>A task representing the asynchronous operation returning <typeparamref name="TResponse"/>.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
