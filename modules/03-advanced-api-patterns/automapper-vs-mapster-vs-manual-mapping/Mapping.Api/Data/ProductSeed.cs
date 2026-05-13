using Mapping.Api.Domain;

namespace Mapping.Api.Data;

public static class ProductSeed
{
    public static Product Sample() => new()
    {
        Id = Guid.Parse("a4f6c1d2-3e5b-4a7c-8d9e-1f2a3b4c5d6e"),
        Name = "Wireless Mechanical Keyboard",
        Description = "Hot-swappable RGB keyboard with 75% layout and Bluetooth 5.3.",
        Price = 169.99m,
        StockQuantity = 42,
        CreatedAt = new DateTime(2026, 1, 14, 9, 30, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 5, 2, 14, 12, 0, DateTimeKind.Utc),
        Category = new Category { Id = 7, Name = "Peripherals", Slug = "peripherals" },
        Tags =
        [
            new Tag { Id = 1, Label = "wireless" },
            new Tag { Id = 2, Label = "rgb" },
            new Tag { Id = 3, Label = "mechanical" }
        ]
    };
}
