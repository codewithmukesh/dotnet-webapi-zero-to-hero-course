namespace MovieManagement.Domain.Common;

// Thrown when a domain rule (an invariant) is violated. The API translates this
// into a 400 Bad Request, so invalid input never surfaces as a 500.
public sealed class DomainException(string message) : Exception(message);
