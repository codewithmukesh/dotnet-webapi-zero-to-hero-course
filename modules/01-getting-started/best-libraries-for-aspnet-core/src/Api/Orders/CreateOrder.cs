using Api.Data;
using Mediator;

namespace Api.Orders;

public record CreateOrder(Guid ProductId, int Quantity) : IRequest<OrderCreated>;

public record OrderCreated(Guid OrderId, decimal Total);

public class CreateOrderHandler(AppDbContext db, OrderCalculator calculator)
    : IRequestHandler<CreateOrder, OrderCreated>
{
    public async ValueTask<OrderCreated> Handle(CreateOrder request, CancellationToken ct)
    {
        var total = await calculator.TotalAsync(request.ProductId, request.Quantity, ct);

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Total = total,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        return new OrderCreated(order.Id, order.Total);
    }
}
