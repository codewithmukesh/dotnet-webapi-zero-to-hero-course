using HybridCaching.Api.Data;
using HybridCaching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace HybridCaching.Api.Services;

public class ProductService(AppDbContext context, HybridCache cache, ILogger<ProductService> logger) : IProductService
{
    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            "products",
            async ct =>
            {
                logger.LogInformation("Cache miss for key: products. Fetching from database.");
                return await context.Products.AsNoTracking().ToListAsync(ct);
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(30)
            },
            tags: ["products"],
            cancellationToken: cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            $"product:{id}",
            async ct =>
            {
                logger.LogInformation("Cache miss for key: product:{ProductId}. Fetching from database.", id);
                return await context.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id, ct);
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
                Expiration = TimeSpan.FromMinutes(20)
            },
            tags: ["products"],
            cancellationToken: cancellationToken);
    }

    public async Task<List<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            $"products:category:{category}",
            async ct =>
            {
                logger.LogInformation("Cache miss for key: products:category:{Category}. Fetching from database.", category);
                return await context.Products
                    .AsNoTracking()
                    .Where(p => p.Category == category)
                    .ToListAsync(ct);
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(30)
            },
            tags: ["products", $"category:{category}"],
            cancellationToken: cancellationToken);
    }

    public async Task<Product> CreateAsync(ProductCreationDto request, CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Name, request.Description, request.Price, request.Category);
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Tag-based invalidation: remove all entries tagged with "products"
        logger.LogInformation("Invalidating all cache entries tagged with 'products'.");
        await cache.RemoveByTagAsync("products", cancellationToken);

        return product;
    }

    public async Task<Product?> UpdateAsync(Guid id, ProductCreationDto request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FindAsync([id], cancellationToken);
        if (product is null) return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Category = request.Category;

        await context.SaveChangesAsync(cancellationToken);

        // Invalidate the individual product and all list caches
        logger.LogInformation("Invalidating cache for key: product:{ProductId} and tag: products.", id);
        await cache.RemoveAsync($"product:{id}", cancellationToken);
        await cache.RemoveByTagAsync("products", cancellationToken);

        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invalidating cache for key: product:{ProductId} and tag: products.", id);
        await cache.RemoveAsync($"product:{id}", cancellationToken);
        await cache.RemoveByTagAsync("products", cancellationToken);

        return true;
    }
}
