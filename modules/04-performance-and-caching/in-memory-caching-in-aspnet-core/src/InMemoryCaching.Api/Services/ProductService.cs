using InMemoryCaching.Api.Data;
using InMemoryCaching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCaching.Api.Services;

public class ProductService(AppDbContext context, IMemoryCache cache, ILogger<ProductService> logger) : IProductService
{
    private const string AllProductsCacheKey = "products";

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Pattern 1: GetOrCreateAsync — the clean, modern approach
        var products = await cache.GetOrCreateAsync(AllProductsCacheKey, async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(30))
                 .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                 .SetPriority(CacheItemPriority.High)
                 .SetSize(2048);

            logger.LogInformation("Cache miss for key: {CacheKey}. Fetching from database.", AllProductsCacheKey);
            return await context.Products.AsNoTracking().ToListAsync(cancellationToken);
        });

        return products ?? [];
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{id}";

        // Pattern 2: Manual TryGetValue + Set — for fine-grained control
        if (cache.TryGetValue(cacheKey, out Product? product))
        {
            logger.LogInformation("Cache hit for key: {CacheKey}.", cacheKey);
            return product;
        }

        logger.LogInformation("Cache miss for key: {CacheKey}. Fetching from database.", cacheKey);
        product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is not null)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1)
                .RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    logger.LogInformation("Cache entry evicted. Key: {CacheKey}, Reason: {Reason}.", key, reason);
                });

            cache.Set(cacheKey, product, cacheOptions);
        }

        return product;
    }

    public async Task<Product> CreateAsync(ProductCreationDto request, CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Invalidate the products list cache since a new product was added
        logger.LogInformation("Invalidating cache for key: {CacheKey}.", AllProductsCacheKey);
        cache.Remove(AllProductsCacheKey);

        return product;
    }
}
