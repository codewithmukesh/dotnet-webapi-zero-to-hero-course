using BenchmarkDotNet.Attributes;
using InMemoryCaching.Api.Data;
using InMemoryCaching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCaching.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class CacheBenchmarks
{
    private AppDbContext _dbContext = null!;
    private IMemoryCache _cache = null!;
    private Guid _productId;
    private List<Product> _allProducts = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=inmemorycaching;Username=postgres;Password=admin")
            .Options;

        _dbContext = new AppDbContext(options);

        // Ensure data exists
        _allProducts = _dbContext.Products.AsNoTracking().Take(1000).ToList();
        if (_allProducts.Count == 0)
        {
            throw new InvalidOperationException("No products found. Run the API and seed data first.");
        }

        _productId = _allProducts.First().Id;

        // Set up cache with pre-warmed entries
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10_000 });

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSize(1);

        _cache.Set($"product:{_productId}", _allProducts.First(), cacheOptions);

        var listOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSize(2048);

        _cache.Set("products", _allProducts, listOptions);
    }

    [Benchmark(Baseline = true)]
    public async Task<Product?> SingleProduct_DatabaseFetch()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _productId);
    }

    [Benchmark]
    public Product? SingleProduct_CacheHit()
    {
        _cache.TryGetValue($"product:{_productId}", out Product? product);
        return product;
    }

    [Benchmark]
    public async Task<List<Product>> AllProducts_DatabaseFetch()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Take(1000)
            .ToListAsync();
    }

    [Benchmark]
    public List<Product>? AllProducts_CacheHit()
    {
        _cache.TryGetValue("products", out List<Product>? products);
        return products;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cache.Dispose();
        _dbContext.Dispose();
    }
}
