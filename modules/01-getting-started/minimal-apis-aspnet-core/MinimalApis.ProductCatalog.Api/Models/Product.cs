namespace MinimalApis.ProductCatalog.Api.Models;

public record Product(int Id, string Name, string Category, decimal Price, int Stock);
