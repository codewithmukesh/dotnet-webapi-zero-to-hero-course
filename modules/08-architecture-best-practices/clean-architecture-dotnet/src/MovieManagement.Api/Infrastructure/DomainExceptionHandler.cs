using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MovieManagement.Domain.Common;

namespace MovieManagement.Api.Infrastructure;

// Catches a DomainException anywhere in the request and returns a 400 Bad Request
// with a clean ProblemDetails body, instead of letting it become a 500.
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
        {
            // Not a domain rule violation - let the default handler return a 500.
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = exception.Message
            }
        });
    }
}
