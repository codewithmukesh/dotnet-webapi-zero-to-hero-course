namespace EfCoreBulkInsert.Shared;

// A deliberately ordinary 8-column entity. Wide enough to make the
// SQL Server 2,100-parameter ceiling reachable at realistic batch sizes.
public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public required string Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
