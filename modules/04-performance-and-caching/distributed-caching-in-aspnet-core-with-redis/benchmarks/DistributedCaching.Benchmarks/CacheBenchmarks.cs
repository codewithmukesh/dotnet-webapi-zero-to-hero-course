using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using DistributedCaching.Api.Data;
using DistributedCaching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace DistributedCaching.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class CacheBenchmarks
{
    private AppDbContext _dbContext = null!;
    private IMemoryCache _memoryCache = null!;
    private IDistributedCache _redisCache = null!;
    private Guid _productId;
    private List<Product> _allProducts = null!;

    [GlobalSetup]
    public void Setup()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=distributedcaching;Username=postgres;Password=admin")
            .Options;

        _dbContext = new AppDbContext(dbOptions);

        _allProducts = _dbContext.Products.AsNoTracking().Take(1000).ToList();
        if (_allProducts.Count == 0)
            throw new InvalidOperationException("No products found. Seed the database first.");

        _productId = _allProducts.First().Id;

        // Setup in-memory cache
        _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10_000 });
        var memOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSize(2048);
        _memoryCache.Set("products", _allProducts, memOptions);
        _memoryCache.Set($"product:{_productId}", _allProducts.First(),
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)).SetSize(1));

        // Setup Redis cache
        _redisCache = new RedisCache(Options.Create(new RedisCacheOptions
        {
            Configuration = "localhost:6379"
        }));

        // Pre-warm Redis
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_allProducts.First()));
        _redisCache.Set($"product:{_productId}", bytes, new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));

        var listBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_allProducts));
        _redisCache.Set("products", listBytes, new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
    }

    [Benchmark(Baseline = true)]
    public async Task<Product?> SingleProduct_DatabaseFetch()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == _productId);
    }

    [Benchmark]
    public Product? SingleProduct_MemoryCacheHit()
    {
        _memoryCache.TryGetValue($"product:{_productId}", out Product? product);
        return product;
    }

    [Benchmark]
    public Product? SingleProduct_RedisCacheHit()
    {
        var bytes = _redisCache.Get($"product:{_productId}");
        return bytes is not null ? JsonSerializer.Deserialize<Product>(bytes) : null;
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
    public List<Product>? AllProducts_MemoryCacheHit()
    {
        _memoryCache.TryGetValue("products", out List<Product>? products);
        return products;
    }

    [Benchmark]
    public List<Product>? AllProducts_RedisCacheHit()
    {
        var bytes = _redisCache.Get("products");
        return bytes is not null ? JsonSerializer.Deserialize<List<Product>>(bytes) : null;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _memoryCache.Dispose();
        _dbContext.Dispose();
    }
}
