namespace TaxTrack.Api.Middleware;

/// <summary>
/// Middleware that extracts or generates a correlation ID for distributed request tracing.
/// </summary>
/// <remarks>
/// If the request includes an X-Correlation-ID header, it is extracted and propagated.
/// Otherwise, the ASP.NET Core TraceIdentifier is used.
/// The correlation ID is included in response headers and logging scope for end-to-end traceability.
/// This is critical for POPIA audit compliance and incident investigation.
/// </remarks>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware to establish correlation ID context for the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the middleware execution.</returns>
    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var value) &&
                            !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : context.TraceIdentifier;

        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }
    }
}
