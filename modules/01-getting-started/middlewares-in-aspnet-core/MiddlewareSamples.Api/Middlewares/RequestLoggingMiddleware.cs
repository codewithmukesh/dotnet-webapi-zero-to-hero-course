namespace MiddlewareSamples.Api.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation("Incoming {Method} {Path}", context.Request.Method, context.Request.Path);

        await next(context);

        var elapsed = DateTime.UtcNow - startTime;
        logger.LogInformation("Completed {Method} {Path} with {StatusCode} in {Elapsed}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed.TotalMilliseconds);
    }
}
