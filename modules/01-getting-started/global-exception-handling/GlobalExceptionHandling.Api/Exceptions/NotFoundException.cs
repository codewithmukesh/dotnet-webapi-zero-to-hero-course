using System.Net;

namespace GlobalExceptionHandling.Api.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// Returns HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException(string resourceName, object key)
    : AppException($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound);
