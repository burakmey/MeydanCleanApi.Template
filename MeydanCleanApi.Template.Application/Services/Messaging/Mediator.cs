using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace MeydanCleanApi.Template.Application.Services.Messaging;

/// <summary>
/// Custom, high-performance license-free Mediator implementation.
/// Dispatches CQRS requests to their registered handlers and executes pipeline behaviors sequentially.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Mediator"/> class.
/// </remarks>
/// <param name="serviceProvider">Dependency injection container service provider.</param>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for request type '{requestType.Name}'.");
        var pipelineType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(pipelineType)
            .Cast<dynamic>()
            .Reverse()
            .ToList();

        RequestHandlerDelegate<TResponse> executionPipeline = () =>
        {
            var method = handlerType.GetMethod("Handle")!;
            return (Task<TResponse>)method.Invoke(handler, [request, ct])!;
        };

        foreach (var behavior in behaviors)
        {
            var currentPipeline = executionPipeline;
            executionPipeline = () => behavior.Handle((dynamic)request, currentPipeline, ct);
        }

        return await executionPipeline();
    }
}
