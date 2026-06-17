namespace RoleBasedAuth.Api.Models;

public record Product(int Id, string Name, int Stock);

public record CreateProductRequest(string Name, int Stock);
