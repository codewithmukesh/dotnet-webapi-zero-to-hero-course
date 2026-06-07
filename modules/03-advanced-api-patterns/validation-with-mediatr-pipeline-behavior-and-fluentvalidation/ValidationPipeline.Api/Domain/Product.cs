namespace ValidationPipeline.Api.Domain;

public class Product
{
    public Guid Id { get; init; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }

    private Product() { }

    public Product(string name, string description, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
    }
}
