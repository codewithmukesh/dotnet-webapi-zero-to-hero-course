using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using HybridCaching.Api.Data;
using HybridCaching.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HybridCaching.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class CacheBenchmarks
{
    private AppDbContext _dbContext = null!;
    private IMemoryCache _memoryCache = null!;
    private IDistributedCache _redisCache = null!;
    private HybridCache _hybridCache = null!;
    private Guid _productId;
    private List<Product> _allProducts = null!;

    [GlobalSetup]
    public void Setup()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=hybridcaching;Username=postgres;Password=admin")
            .Options;

        _dbContext = new AppDbContext(dbOptions);
        _allProducts = _dbContext.Products.AsNoTracking().Take(1000).ToList();
        if (_allProducts.Count == 0)
            throw new InvalidOperationException("No products found. Seed the database first.");

        _productId = _allProducts.First().Id;

        // Setup IMemoryCache
        _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10_000 });
        _memoryCache.Set("products", _allProducts,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)).SetSize(2048));
        _memoryCache.Set($"product:{_productId}", _allProducts.First(),
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)).SetSize(1));

        // Setup Redis (IDistributedCache)
        _redisCache = new RedisCache(Options.Create(new RedisCacheOptions
        {
            Configuration = "localhost:6379"
        }));

        // Pre-warm Redis
        var singleBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_allProducts.First()));
        _redisCache.Set($"product:{_productId}", singleBytes,
            new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
        var listBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_allProducts));
        _redisCache.Set("products", listBytes,
            new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));

        // Setup HybridCache
        var services = new ServiceCollection();
        #pragma warning disable EXTEXP0018
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new()
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(30),
                Expiration = TimeSpan.FromHours(1)
            };
        });
        #pragma warning restore EXTEXP0018
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "localhost:6379";
        });
        var provider = services.BuildServiceProvider();
        _hybridCache = provider.GetRequiredService<HybridCache>();

        // Pre-warm HybridCache (L1 + L2)
        _hybridCache.GetOrCreateAsync($"product:{_productId}",
            _ => ValueTask.FromResult(_allProducts.First())).AsTask().Wait();
        _hybridCache.GetOrCreateAsync("products",
            _ => ValueTask.FromResult(_allProducts)).AsTask().Wait();
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
    public async Task<Product?> SingleProduct_HybridCacheHit()
    {
        return await _hybridCache.GetOrCreateAsync(
            $"product:{_productId}",
            _ => ValueTask.FromResult<Product?>(null));
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

    [Benchmark]
    public async Task<List<Product>> AllProducts_HybridCacheHit()
    {
        return await _hybridCache.GetOrCreateAsync(
            "products",
            _ => ValueTask.FromResult(new List<Product>()));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _memoryCache.Dispose();
        _dbContext.Dispose();
    }
}
