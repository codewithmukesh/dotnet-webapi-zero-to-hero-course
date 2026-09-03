namespace ResultPattern.Api.Orders;

public sealed record Order
{
    public Guid Id { get; init; }
    public required string CustomerEmail { get; init; }
    public decimal Total { get; init; }

    // Settable because cancelling mutates a tracked entity.
    public string Status { get; set; } = "Pending";
}

public sealed record CreateOrderRequest(string CustomerEmail, decimal Total);

public sealed record Refund
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}
