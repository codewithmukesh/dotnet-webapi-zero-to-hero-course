namespace ConcurrencyControl.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    public uint RowVersion { get; set; }
}
