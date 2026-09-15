using Serilog.Context;

namespace MeydanCleanApi.Template.WebApi.Middlewares;

/// <summary>
/// Gives every incoming HTTP request a correlation ID, pushes it to Serilog diagnostic context, and appends it to response headers.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Execution Priority:</strong> Must be registered first in the HTTP pipeline (before ExceptionHandler), so any log line or failure
/// produced during request processing is tagged with the correlation ID.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// 1. Reads incoming <c>X-Correlation-ID</c> header or generates a new Guid string if missing/malformed.
/// 2. Accepts only short, plain identifiers, so a client cannot write its own lines into the log.
/// 3. Pushes property to Serilog <c>LogContext</c> so all log statements automatically include <c>CorrelationId</c>.
/// 4. Appends header to outgoing response headers early so error responses also carry the correlation trace ID.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
/// </remarks>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>The HTTP header carrying the correlation ID in both request and response.</summary>
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>Maximum allowed length for an inbound correlation ID.</summary>
    private const int MaxLength = 128;

    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Resolves correlation ID, sets context, pushes to Serilog, and executes pipeline.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, ICorrelationIdContext correlationIdContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationIdContext);

        var correlationId = ResolveCorrelationId(context);

        correlationIdContext.Set(correlationId);

        // Appends header before response starts so error responses also contain the header
        context.Response.Headers[HeaderName] = correlationId;

        // Pushes CorrelationId property to Serilog LogContext for all logs written during this request
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var header))
        {
            var candidate = header.ToString();
            if (candidate.Length is > 0 and <= MaxLength && IsPlainIdentifier(candidate))
            {
                return candidate;
            }
        }

        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Returns whether the value is safe to write into a log line and a response header.
    /// </summary>
    /// <remarks>
    /// A length limit alone is not enough. The value is written to the log file, so a carriage return
    /// inside it would end the current line and let the caller compose a convincing entry of its own.
    /// Correlation ids are machine-generated identifiers, so accepting only letters, digits, dash and
    /// underscore costs nothing and leaves no character that could mean something to a reader.
    /// </remarks>
    private static bool IsPlainIdentifier(string value)
    {
        foreach (var character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_')
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Pipeline extension methods for registering <see cref="CorrelationIdMiddleware"/>.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds correlation ID tracking middleware to the HTTP pipeline.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
