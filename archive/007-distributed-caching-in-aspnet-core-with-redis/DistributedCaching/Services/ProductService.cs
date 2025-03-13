using DistributedCaching.Models;
using DistributedCaching.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedCaching.Services;

public class ProductService(AppDbContext context, IDistributedCache cache, ILogger<ProductService> logger) : IProductService
{
    public async Task Add(ProductCreationDto request)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        // invalidate cache for products, as new product is added
        var cacheKey = "products";
        logger.LogInformation("invalidating cache for key: {CacheKey} from cache.", cacheKey);
        cache.Remove(cacheKey);
    }

    public async Task<Product> Get(Guid id)
    {
        var cacheKey = $"product:{id}";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        var product = await cache.GetOrSetAsync(cacheKey,
            async () =>
            {
                logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
                return await context.Products.FindAsync(id)!;
            })!;
        return product!;
    }

    public async Task<List<Product>> GetAll()
    {
        var cacheKey = "products";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));
        var products = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
                return await context.Products.ToListAsync();
            },
            cacheOptions)!;
        return products!;
    }
}
