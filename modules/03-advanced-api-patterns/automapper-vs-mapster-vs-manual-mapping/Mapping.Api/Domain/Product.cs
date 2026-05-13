namespace Mapping.Api.Domain;

public sealed class Product
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public Category Category { get; set; } = null!;
    public List<Tag> Tags { get; set; } = new();
}

public sealed class Category
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public sealed class Tag
{
    public int Id { get; init; }
    public string Label { get; set; } = string.Empty;
}
