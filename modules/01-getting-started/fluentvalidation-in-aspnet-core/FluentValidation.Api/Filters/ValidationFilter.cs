using FluentValidation;

namespace FluentValidation.Api.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices
            .GetService<IValidator<T>>();

        if (validator is null)
        {
            return await next(context);
        }

        var model = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (model is null)
        {
            return Microsoft.AspNetCore.Http.Results.Problem(
                "Request body is required.", statusCode: 400);
        }

        var validationResult = await validator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            return Microsoft.AspNetCore.Http.Results.ValidationProblem(
                validationResult.ToDictionary());
        }

        return await next(context);
    }
}
