namespace Api.Products;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record ProductDto(Guid Id, string Name, string Sku, decimal Price);

public record CreateProductRequest(string Name, string Sku, decimal Price);
