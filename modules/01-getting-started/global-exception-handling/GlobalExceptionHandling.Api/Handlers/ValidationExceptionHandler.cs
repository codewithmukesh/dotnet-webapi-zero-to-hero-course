using GlobalExceptionHandling.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GlobalExceptionHandling.Api.Handlers;

/// <summary>
/// Specialized handler for ValidationException.
/// Returns ValidationProblemDetails with field-level error information.
/// </summary>
public sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation)
        {
            return false;
        }

        logger.LogWarning("Validation failed: {Message}", validation.Message);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(validation.Errors)
        {
            Status = 400,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
