namespace PermissionBasedAuth.Api.Entities;

// Carried over from the role-based authorization article in this series.
// Roles still exist - they are just the container permissions hang off now.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static IReadOnlyList<string> All { get; } = [Admin, Manager, User];
}
