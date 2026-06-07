using MediatR;

namespace ValidationPipeline.Api.Behaviors;

// A pipeline behavior wraps every MediatR request. This one logs the request
// going in and the response coming back, with a correlation id to tie them together.
public class RequestResponseLoggingBehavior<TRequest, TResponse>(
    ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        logger.LogInformation("Handling {RequestName} [{CorrelationId}]: {@Request}",
            typeof(TRequest).Name, correlationId, request);

        // next() passes control to the next behavior, or to the handler itself.
        var response = await next(cancellationToken);

        logger.LogInformation("Handled {RequestName} [{CorrelationId}]",
            typeof(TRequest).Name, correlationId);

        return response;
    }
}
