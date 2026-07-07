using Microsoft.AspNetCore.Identity;

namespace CustomUserManagement.Api.Models;

// This is the whole point of "custom" user management. IdentityUser gives you Id, Email,
// UserName, PasswordHash, and lockout columns. You inherit from it and add the fields your
// app actually needs. MapIdentityApi cannot do this - its /register body is fixed at
// { email, password }, so custom fields are the first reason you build your own endpoints.
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    // A custom "is this account allowed to log in" flag. This is NOT the same as Identity
    // lockout - see the article's "Lock vs Disable" section. Disable = a permanent admin
    // switch; lockout = a temporary security response to failed logins.
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
}
