using System.Reflection;
using FluentValidation;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Application.Behaviors;
using MeydanCleanApi.Template.Application.Services.FileStorage;
using MeydanCleanApi.Template.Application.Services.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace MeydanCleanApi.Template.Application;

/// <summary>
/// Dependency Injection extension methods for registering Application layer services, mediator engine, pipeline behaviors, and validators.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers all Application layer services, lightweight custom Mediator engine, pipeline behaviors, and FluentValidation rules.
    /// </summary>
    /// <param name="services">The ServiceCollection instance.</param>
    /// <returns>The ServiceCollection instance for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. Register Custom Zero-Dependency Mediator Engine (Concrete class in Application/Services/Messaging)
        services.AddScoped<IMediator, Mediator>();

        // 2. Register Pipeline Behaviors (Order: Logging -> Performance -> Validation)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 3. Register Application Orchestrator Services
        services.AddScoped<IFileStorageCoordinator, FileStorageCoordinator>();

        // 4. Scan & Register All IRequestHandler<,> implementations in Assembly
        RegisterRequestHandlers(services, assembly);

        // 5. Scan & Register All FluentValidation IValidator<T> implementations
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    private static void RegisterRequestHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler.Interface, handler.Implementation);
        }
    }
}
