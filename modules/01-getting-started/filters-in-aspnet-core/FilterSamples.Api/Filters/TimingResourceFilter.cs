using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FilterSamples.Api.Filters;

public class TimingResourceFilter(ILogger<TimingResourceFilter> logger) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var actionName = context.ActionDescriptor.DisplayName;

        logger.LogInformation("Starting request pipeline for {ActionName}", actionName);

        var executedContext = await next();

        stopwatch.Stop();
        logger.LogInformation("Completed {ActionName} in {ElapsedMs}ms",
            actionName, stopwatch.ElapsedMilliseconds);
    }
}
