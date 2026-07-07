using CustomUserManagement.Api.Contracts;

namespace CustomUserManagement.Api.Models;

// One place that turns an ApplicationUser + its roles into the safe response DTO.
// Keeps every endpoint from hand-mapping (and accidentally leaking PasswordHash).
public static class UserMapping
{
    public static UserResponse ToResponse(this ApplicationUser user, IEnumerable<string> roles) =>
        new(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.ProfilePictureUrl,
            user.IsActive,
            // LockoutEnd in the future means the account is currently locked out.
            IsLockedOut: user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow,
            user.CreatedOn,
            roles);
}
