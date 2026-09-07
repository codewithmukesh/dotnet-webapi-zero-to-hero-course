namespace EfCoreContainsLargeList.Shared;

/// <summary>
/// A composite-key entity: stock is identified by (TenantId, ProductId), not by a surrogate id.
/// This is the shape that has no <c>IN</c> form.
/// </summary>
public class InventoryItem
{
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
