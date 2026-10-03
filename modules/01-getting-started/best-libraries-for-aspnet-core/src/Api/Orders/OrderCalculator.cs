using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Orders;

public interface IPriceService
{
    Task<decimal> GetPriceAsync(Guid productId, CancellationToken ct);
}

public class ProductPriceService(AppDbContext db) : IPriceService
{
    public async Task<decimal> GetPriceAsync(Guid productId, CancellationToken ct) =>
        await db.Products
            .Where(p => p.Id == productId)
            .Select(p => p.Price)
            .SingleAsync(ct);
}

public class OrderCalculator(IPriceService prices)
{
    public async Task<decimal> TotalAsync(Guid productId, int quantity, CancellationToken ct)
    {
        var price = await prices.GetPriceAsync(productId, ct);
        return price * quantity;
    }
}
