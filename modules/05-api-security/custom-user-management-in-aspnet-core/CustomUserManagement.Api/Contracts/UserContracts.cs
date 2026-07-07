namespace CustomUserManagement.Api.Contracts;

// ---- Auth ----
// Custom registration request. Notice FirstName and LastName - the exact fields MapIdentityApi
// cannot accept. This is the whole reason for a custom /register endpoint.
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, DateTime ExpiresAt);

// ---- User responses (DTOs) ----
// Never return IdentityUser/ApplicationUser directly. It carries PasswordHash, security stamps,
// and lockout internals you do not want on the wire. Map to a response shape you control.
public record UserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? ProfilePictureUrl,
    bool IsActive,
    bool IsLockedOut,
    DateTimeOffset CreatedOn,
    IEnumerable<string> Roles);

// A tiny generic envelope so the list endpoint returns items plus paging info.
public record PagedResult<T>(IEnumerable<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ---- Admin actions ----
public record UpdateUserRequest(string FirstName, string LastName);

public record AssignRoleRequest(string Role);

// ---- Self-service (/me) ----
public record UpdateProfileRequest(string FirstName, string LastName, string? ProfilePictureUrl);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

// ---- Email confirmation + password reset ----
public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record ConfirmEmailRequest(string Email, string Token);
