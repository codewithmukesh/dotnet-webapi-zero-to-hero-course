namespace PermissionBasedAuth.Api.Entities;

// Permissions are plain strings. Constants give you compile-time safety at the
// endpoint and a single place to rename them.
public static class Permissions
{
    // Every permission claim uses this type. The policy provider keys off the
    // VALUE prefix below, not off this type.
    public const string ClaimType = "permission";

    // Policy names that start with this prefix get a policy generated on demand.
    public const string Prefix = "Permissions.";

    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Products.View,
        Products.Create,
        Products.Edit,
        Products.Delete
    ];

    // Used when seeding a brand new module so you don't hand-write four constants.
    public static IReadOnlyList<string> ForModule(string module) =>
    [
        $"{Prefix}{module}.View",
        $"{Prefix}{module}.Create",
        $"{Prefix}{module}.Edit",
        $"{Prefix}{module}.Delete"
    ];
}
