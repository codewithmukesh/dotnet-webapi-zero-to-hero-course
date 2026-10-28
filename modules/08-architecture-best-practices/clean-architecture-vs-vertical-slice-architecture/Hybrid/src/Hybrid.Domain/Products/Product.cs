using Hybrid.Domain.Common;

namespace Hybrid.Domain.Products;

public sealed class Product
{
    private Product()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public uint Version { get; private set; }

    public static Product Create(string name, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name is required.");
        }

        if (price <= 0)
        {
            throw new DomainException("Price must be greater than zero.");
        }

        if (stock < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        return new Product
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            throw new DomainException("Price must be greater than zero.");
        }

        Price = newPrice;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (quantity > Stock)
        {
            throw new DomainException($"Cannot remove {quantity} items. Only {Stock} left in stock.");
        }

        Stock -= quantity;
    }
}
