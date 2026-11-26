namespace WebAPI.Common.Diagnostics;

internal sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    // The file log template draws a separator before every event with this EventId (see appsettings.json).
    private static readonly EventId RequestStarted = new(1, nameof(RequestStarted));

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            return Task.CompletedTask;
        });

        using var scope = logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });
        logger.LogInformation(
            RequestStarted,
            "{RequestMethod} {RequestPath}{QueryString}  (CorrelationId: {CorrelationId})",
            context.Request.Method,
            context.Request.Path.Value,
            context.Request.QueryString.Value,
            correlationId);

        await next(context);
    }
}
