namespace FilterSamples.Api.EndpointFilters;

public class ValidationEndpointFilter<T>(ILogger<ValidationEndpointFilter<T>> logger) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
        {
            logger.LogWarning("Request body of type {Type} is null", typeof(T).Name);
            return Results.BadRequest($"Request body of type {typeof(T).Name} is required.");
        }

        logger.LogInformation("Validated endpoint argument of type {Type}", typeof(T).Name);
        return await next(context);
    }
}
