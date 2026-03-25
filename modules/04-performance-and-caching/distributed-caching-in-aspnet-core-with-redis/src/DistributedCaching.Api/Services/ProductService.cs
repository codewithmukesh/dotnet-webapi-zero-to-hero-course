using DistributedCaching.Api.Data;
using DistributedCaching.Api.Extensions;
using DistributedCaching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DistributedCaching.Api.Services;

public class ProductService(AppDbContext context, IDistributedCache cache, ILogger<ProductService> logger) : IProductService
{
    private const string AllProductsCacheKey = "products";

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        var products = await cache.GetOrSetAsync(
            AllProductsCacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for key: {CacheKey}. Fetching from database.", AllProductsCacheKey);
                return await context.Products.AsNoTracking().ToListAsync(cancellationToken);
            },
            cacheOptions,
            cancellationToken);

        return products ?? [];
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{id}";

        var product = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for key: {CacheKey}. Fetching from database.", cacheKey);
                return await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            },
            cancellationToken: cancellationToken);

        return product;
    }

    public async Task<Product> CreateAsync(ProductCreationDto request, CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invalidating cache for key: {CacheKey}.", AllProductsCacheKey);
        await cache.RemoveAsync(AllProductsCacheKey, cancellationToken);

        return product;
    }
}
