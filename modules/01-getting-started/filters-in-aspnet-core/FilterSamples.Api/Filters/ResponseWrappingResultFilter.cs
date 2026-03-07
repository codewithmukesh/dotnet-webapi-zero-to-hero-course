using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FilterSamples.Api.Filters;

public class ResponseWrappingResultFilter(ILogger<ResponseWrappingResultFilter> logger) : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode is null or (>= 200 and < 300))
        {
            objectResult.Value = new
            {
                success = true,
                data = objectResult.Value,
                timestamp = DateTime.UtcNow
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        logger.LogDebug("Result executed for {ActionName}", context.ActionDescriptor.DisplayName);
    }
}
