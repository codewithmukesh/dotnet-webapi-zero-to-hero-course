using Microsoft.AspNetCore.Http.HttpResults;

namespace ResultPattern.Api.Common;

public static class ResultExtensions
{
    /// <summary>
    /// The single place where a domain Error becomes an HTTP response.
    /// Every endpoint funnels through here, so the error contract stays consistent.
    /// </summary>
    public static ProblemHttpResult ToProblem(this Error error) => error.Type switch
    {
        ErrorType.Validation => TypedResults.Problem(
            title: "Validation failed",
            detail: error.Description,
            statusCode: StatusCodes.Status400BadRequest,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code }),

        ErrorType.NotFound => TypedResults.Problem(
            title: "Resource not found",
            detail: error.Description,
            statusCode: StatusCodes.Status404NotFound,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code }),

        ErrorType.Conflict => TypedResults.Problem(
            title: "Conflict",
            detail: error.Description,
            statusCode: StatusCodes.Status409Conflict,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code }),

        ErrorType.Forbidden => TypedResults.Problem(
            title: "Forbidden",
            detail: error.Description,
            statusCode: StatusCodes.Status403Forbidden,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code }),

        _ => TypedResults.Problem(
            title: "An error occurred",
            detail: error.Description,
            statusCode: StatusCodes.Status500InternalServerError,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code })
    };

    /// <summary>
    /// Result&lt;T&gt; carries one Error. When a request has several validation
    /// failures at once, map the whole list to a single RFC 9457 response with an
    /// errors dictionary, keyed by the stable error code.
    /// </summary>
    public static ProblemHttpResult ToValidationProblem(this IReadOnlyList<Error> errors) =>
        TypedResults.Problem(new HttpValidationProblemDetails(
            errors
                .GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(e => e.Description).ToArray()))
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest
        });

    /// <summary>
    /// Chains one async Result-returning step onto another. One overload covers
    /// Task&lt;Result&lt;T&gt;&gt; only - the non-generic Result, the sync case and Map/Tap
    /// each need their own, which is the cost catch #4 describes.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> task,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await task;
        return result.IsFailure ? result.Error : await next(result.Value);
    }
}
