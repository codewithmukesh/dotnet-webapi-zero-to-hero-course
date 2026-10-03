namespace Api.Orders;

public class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record OrderSummary(Guid Id, decimal Total, string Status);
