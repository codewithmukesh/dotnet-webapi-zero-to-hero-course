using InMemoryCaching.Models;
using InMemoryCaching.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCaching.Services;

public class ProductService(AppDbContext context, IMemoryCache cache, ILogger<ProductService> logger) : IProductService
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
        if (!cache.TryGetValue(cacheKey, out Product? product))
        {
            logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
            product = await context.Products.FindAsync(id);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(50))
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetPriority(CacheItemPriority.Normal);
            logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
            cache.Set(cacheKey, product, cacheOptions);
        }
        else
        {
            logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
        }
        return product;
    }

    public async Task<List<Product>> GetAll()
    {
        var cacheKey = "products";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        if (!cache.TryGetValue(cacheKey, out List<Product>? products))
        {
            logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
            products = await context.Products.ToListAsync();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetPriority(CacheItemPriority.NeverRemove)
                .SetSize(2048);
            logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
            cache.Set(cacheKey, products, cacheOptions);
        }
        else
        {
            logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
        }
        return products;
    }
}
