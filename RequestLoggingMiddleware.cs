using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation id
        var correlationId = Guid.NewGuid()
            .ToString("N")[..8];

        // Add correlation id to response header
        context.Response.Headers["X-Correlation-Id"] =
            correlationId;

        // Start timer
        var stopwatch = Stopwatch.StartNew();

        // Log request entry
        _logger.LogInformation(
            "Incoming Request => Method: {Method}, Path: {Path}, CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // Call next middleware
        await _next(context);

        // Stop timer
        stopwatch.Stop();

        // Log request exit
        _logger.LogInformation(
            "Outgoing Response => StatusCode: {StatusCode}, ElapsedMs: {ElapsedMs}, CorrelationId: {CorrelationId}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}