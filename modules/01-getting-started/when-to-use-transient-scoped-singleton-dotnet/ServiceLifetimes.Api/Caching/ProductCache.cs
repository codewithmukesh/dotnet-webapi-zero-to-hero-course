using Microsoft.EntityFrameworkCore;
using ServiceLifetimes.Api.Data;

namespace ServiceLifetimes.Api.Caching;

// Registered as SINGLETON. It needs the database, but it must not hold on to a scoped AppDbContext.
// Instead it asks IServiceScopeFactory for a fresh scope every time it loads, and the scope
// disposes that DbContext as soon as the load is done.
public sealed class ProductCache(IServiceScopeFactory scopeFactory)
{
    private IReadOnlyList<Product>? _products;

    public Guid InstanceId { get; } = Guid.NewGuid();
    public Guid? LastDbContextId { get; private set; }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(bool refresh, CancellationToken cancellationToken)
    {
        if (_products is not null && !refresh)
        {
            return _products;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        LastDbContextId = db.ContextId.InstanceId;

        _products = await db.Products.AsNoTracking().ToListAsync(cancellationToken);
        return _products;
    }
}
