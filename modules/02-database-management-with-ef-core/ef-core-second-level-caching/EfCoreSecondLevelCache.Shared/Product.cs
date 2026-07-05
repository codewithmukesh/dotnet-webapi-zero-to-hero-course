namespace EfCoreSecondLevelCache.Shared;

// Reference-style data: read often, changes rarely. The canonical fit for a
// second-level cache.
public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
