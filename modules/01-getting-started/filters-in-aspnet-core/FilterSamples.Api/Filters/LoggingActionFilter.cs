using Microsoft.AspNetCore.Mvc.Filters;

namespace FilterSamples.Api.Filters;

public class LoggingActionFilter(ILogger<LoggingActionFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        logger.LogInformation("Executing action {ActionName} with arguments {@Arguments}",
            context.ActionDescriptor.DisplayName,
            context.ActionArguments);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        logger.LogInformation("Executed action {ActionName} — Status code: {StatusCode}",
            context.ActionDescriptor.DisplayName,
            context.HttpContext.Response.StatusCode);
    }
}
