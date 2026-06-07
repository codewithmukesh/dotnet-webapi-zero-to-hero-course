using Microsoft.AspNetCore.Identity;

namespace JwtAuthentication.Api.Entities;

// We extend the built-in IdentityUser so we can store a couple of extra fields.
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
