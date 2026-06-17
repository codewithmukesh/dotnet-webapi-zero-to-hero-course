namespace RoleBasedAuth.Api.Entities;

// Keeping role names in one place avoids typos like "Admin" vs "admin".
// Role names ARE case-sensitive when they end up as claims in a JWT.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
}
