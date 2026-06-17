namespace ClaimsBasedAuth.Api.Models;

// Records keep these request/response shapes short and immutable.
public record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string UserId,
    string Email,
    IEnumerable<string> Roles,
    string Token,
    DateTime ExpiresAt);
