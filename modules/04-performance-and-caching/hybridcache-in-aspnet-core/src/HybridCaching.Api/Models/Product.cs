namespace HybridCaching.Api.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string Category { get; set; } = default!;

    private Product() { }

    public Product(string name, string description, decimal price, string category)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Category = category;
    }
}

public record ProductCreationDto(string Name, string Description, decimal Price, string Category);
