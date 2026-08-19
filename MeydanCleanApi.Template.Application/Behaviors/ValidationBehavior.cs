using FluentValidation;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using ApplicationValidationException = MeydanCleanApi.Template.Domain.Exceptions.ValidationException;

namespace MeydanCleanApi.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatically executing registered FluentValidation rules before dispatching to request handlers.
/// </summary>
/// <typeparam name="TRequest">Incoming request type.</typeparam>
/// <typeparam name="TResponse">Outgoing response type.</typeparam>
/// <remarks>
/// Failures are converted into the application's own <see cref="ApplicationValidationException"/>, which the
/// global exception handler turns into an HTTP 400 with a field-by-field error list. Letting
/// FluentValidation's own exception escape would land in the "unexpected error" branch instead and
/// return a 500 with no detail.
/// </remarks>
/// <param name="validators">Injected FluentValidation rule validators for <typeparamref name="TRequest"/>.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        // Group by property so the client gets every problem with a field at once,
        // instead of fixing one and immediately hitting the next.
        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(f => f.ErrorMessage).Distinct().ToArray());

        throw ApplicationValidationException.From(errors);
    }
}
