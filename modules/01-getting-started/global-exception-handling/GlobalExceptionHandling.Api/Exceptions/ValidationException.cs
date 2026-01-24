using System.Net;

namespace GlobalExceptionHandling.Api.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// Returns HTTP 400 Bad Request with validation error details.
/// </summary>
public sealed class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, [error] }
        };
    }
}
