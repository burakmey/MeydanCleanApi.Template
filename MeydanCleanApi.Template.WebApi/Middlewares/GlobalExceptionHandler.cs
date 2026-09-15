using MeydanCleanApi.Template.Application.Localization;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

namespace MeydanCleanApi.Template.WebApi.Middlewares;

/// <summary>
/// Global ASP.NET Core exception handler intercepting uncaught application exceptions.
/// Localizes error descriptions using the request culture context and serializes a unified JSON error payload.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
/// </remarks>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        // Once the first byte is on the wire the status code and headers are already sent, and
        // writing again throws on top of the original failure. Nothing useful can be returned here,
        // so log it and let the host tear the connection down rather than mask the real exception.
        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "Unhandled System Crash after the response had started at {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            return false;
        }

        var localizer = httpContext.RequestServices.GetRequiredService<ILocalizationService<ErrorMessages>>();
        var validationLocalizer = httpContext.RequestServices.GetRequiredService<ILocalizationService<ValidationMessages>>();
        ErrorResponse response;

        if (exception is AppException appException)
        {
            var localizedMessage = localizer.Get(appException.Error.Code, appException.Error.Args);

            response = new ErrorResponse
            {
                Status = appException.StatusCode,
                ErrorCode = appException.Error.Code,
                Message = localizedMessage,

                // Validators report stable codes such as VAL_NAME_REQUIRED. Translate them here so
                // the client receives readable text while the codes stay out of the response.
                Errors = appException is ValidationException ve ? Localize(ve.Errors, validationLocalizer) : null
            };

            // Log expected business failures at Information level (avoids log pollution)
            _logger.LogInformation(
                "Handled Application Failure: Code = {ErrorCode}, Status = {StatusCode}, Path = {Path}",
                appException.Error.Code,
                appException.StatusCode,
                httpContext.Request.Path);
        }
        else
        {
            // Mask unexpected system crashes (e.g. DbException, NullReferenceException) to prevent sensitive details leakage
            var localizedMessage = localizer.Get(ErrorCodes.InternalServer);

            response = new ErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                ErrorCode = ErrorCodes.InternalServer,
                Message = localizedMessage
            };

            using (LogContext.PushProperty("ExceptionType", exception.GetType().FullName))
            {
                _logger.LogError(exception, "Unhandled System Crash at {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            }
        }

        httpContext.Response.StatusCode = response.Status;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    /// <summary>
    /// Replaces each validation code with its localized text for the current request culture.
    /// </summary>
    private static IDictionary<string, string[]> Localize(
        IDictionary<string, string[]> errors,
        ILocalizationService<ValidationMessages> localizer)
    {
        return errors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Select(localizer.Get).ToArray());
    }
}
