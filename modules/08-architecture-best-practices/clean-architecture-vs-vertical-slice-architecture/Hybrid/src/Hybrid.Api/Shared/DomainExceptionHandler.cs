using Hybrid.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Hybrid.Api.Shared;

internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = "Business rule violated",
                Detail = domainException.Message,
                Status = StatusCodes.Status400BadRequest
            }
        });
    }
}
