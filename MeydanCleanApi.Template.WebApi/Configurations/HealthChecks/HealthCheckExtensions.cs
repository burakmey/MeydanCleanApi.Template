using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeydanCleanApi.Template.WebApi.Configurations.HealthChecks;

/// <summary>
/// Maps the health endpoints. There are three, because "is it alive" and "is it working" are
/// different questions with different audiences.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>Tag applied to checks that talk to a dependency, such as the database.</summary>
    public const string ReadyTag = "ready";

    /// <summary>
    /// Maps <c>/health</c>, <c>/health/ready</c> and <c>/health/details</c>.
    /// </summary>
    /// <param name="app">The application to map the endpoints on.</param>
    /// <returns>The same application, so calls can be chained.</returns>
    public static WebApplication MapApplicationHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // 1. LIVENESS — /health
        //
        // "Is the process up and answering?" Nothing else. Predicate = false means no check runs, so
        // this never touches the database and answers in well under a millisecond.
        //
        // This is what Docker and a load balancer should call. Using a database-backed endpoint for
        // that job is a trap: one slow query and the balancer removes an instance that was fine, or
        // a brief database blip restarts every container at once.
        //
        // Anonymous, because the authorization fallback policy would otherwise demand a token and no
        // load balancer has one.
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();

        // 2. READINESS — /health/ready
        //
        // "Can it actually serve a request?" Runs the checks tagged 'ready', which query the
        // database. Kubernetes uses this to decide when to send traffic to a new instance, and
        // docker-compose uses it for `depends_on: service_healthy`.
        //
        // Still anonymous: orchestrators probe without credentials. It returns only Healthy or
        // Unhealthy, never the reason, so an anonymous caller learns nothing about the internals.
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag)
        }).AllowAnonymous();

        // 3. DIAGNOSTICS — /health/details
        //
        // The full report: every check, its status, how long it took, and why it failed. That is
        // useful to an operator and equally useful to an attacker, since failure messages name
        // internal hosts and components. It therefore requires the Admin policy.
        app.MapHealthChecks("/health/details", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteDetailedReportAsync
        }).RequireAuthorization(PolicyConstants.Admin);

        return app;
    }

    /// <summary>
    /// Writes the full report as JSON, in the same camelCase shape as every other response.
    /// </summary>
    private static async Task WriteDetailedReportAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
                description = entry.Value.Description,
                tags = entry.Value.Tags,
                // The message only, never the stack trace. Enough to see what broke without
                // publishing the internals of the failure.
                error = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
