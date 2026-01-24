using System.Net;

namespace GlobalExceptionHandling.Api.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate entry, concurrent modification).
/// Returns HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException(string message)
    : AppException(message, HttpStatusCode.Conflict);
