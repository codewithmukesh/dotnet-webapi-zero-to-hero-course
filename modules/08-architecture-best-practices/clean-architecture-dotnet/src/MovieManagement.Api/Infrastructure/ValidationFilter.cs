using FluentValidation;

namespace MovieManagement.Api.Infrastructure;

// A reusable endpoint filter: it finds the request argument of type T, runs its
// FluentValidation validator, and short-circuits with a 400 ValidationProblemDetails
// (field-level errors) when the request is invalid. Valid requests fall through
// to the endpoint untouched.
internal sealed class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            return Results.Problem("The request body was missing or could not be read.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            // Returns application/problem+json with an "errors" dictionary keyed by field.
            return Results.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}
