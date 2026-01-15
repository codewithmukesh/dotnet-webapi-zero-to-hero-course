using System.Diagnostics;

namespace MinimalApis.ProductCatalog.Api.Filters;

public class LoggingFilter(ILogger<LoggingFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint()?.DisplayName ?? "Unknown";
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        logger.LogInformation(
            "Executing: {Method} {Path} ({Endpoint})",
            method, path, endpoint);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next(context);
            stopwatch.Stop();

            logger.LogInformation(
                "Completed: {Method} {Path} in {Duration}ms",
                method, path, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(ex,
                "Failed: {Method} {Path} after {Duration}ms",
                method, path, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
