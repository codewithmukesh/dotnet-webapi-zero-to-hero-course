namespace PermissionBasedAuth.Api.Models;

public record Product(int Id, string Name, decimal Price);

public record SaveProductRequest(string Name, decimal Price);

// Admin surface for managing what a role can do, without a redeploy.
public record RolePermissionsResponse(string Role, IEnumerable<string> Permissions);

public record PermissionRequest(string Permission);
