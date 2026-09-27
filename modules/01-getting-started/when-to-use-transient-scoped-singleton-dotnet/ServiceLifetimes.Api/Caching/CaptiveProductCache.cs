using Microsoft.EntityFrameworkCore;
using ServiceLifetimes.Api.Data;

namespace ServiceLifetimes.Api.Caching;

// DON'T DO THIS. A singleton that takes a scoped AppDbContext in its constructor.
// The singleton is created once, so it keeps the very first DbContext it was given for the
// whole life of the app. That is a captive dependency. It is only registered when
// Demo:EnableCaptiveDependency is true, so you can reproduce it on purpose.
public sealed class CaptiveProductCache(AppDbContext db)
{
    public Guid DbContextId => db.ContextId.InstanceId;

    public Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken) =>
        db.Products.AsNoTracking().ToListAsync(cancellationToken);
}
