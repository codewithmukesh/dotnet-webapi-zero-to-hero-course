using BenchmarkDotNet.Attributes;
using EFCoreSecondLevelCacheInterceptor;
using EfCoreSecondLevelCache.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfCoreSecondLevelCache.Benchmarks;

// Compares one repeated read query three ways: straight to the database (no
// cache), served from the in-memory second-level cache, and served from the
// Redis second-level cache. Each cached path lives in its own DI container so
// the two providers don't collide.
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class CacheBenchmarks
{
    private const string ConnectionString =
        "Host=localhost;Port=5434;Database=second_level_cache_db;Username=postgres;Password=postgres";
    private const string RedisConnection = "localhost:6380";
    private const int SeedRows = 5_000;

    private DbContextOptions<AppDbContext> _noCacheOptions = null!;
    private ServiceProvider _memoryProvider = null!;
    private ServiceProvider _redisProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        _noCacheOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using (var context = new AppDbContext(_noCacheOptions))
        {
            context.Database.EnsureCreated();
            if (!context.Products.Any())
            {
                var products = Enumerable.Range(0, SeedRows).Select(i => new Product
                {
                    Name = $"Product {i:D5}",
                    Category = $"Category {i % 20}",
                    Price = 9.99m + i % 500,
                    IsActive = i % 7 != 0
                });
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }

        // In-memory second-level cache provider (its own container).
        _memoryProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddLogging()
            .AddEFSecondLevelCache(options => options
                .UseMemoryCacheProvider()
                .ConfigureLogging(false)
                .UseCacheKeyPrefix("EF_"))
            .BuildServiceProvider();

        // Redis second-level cache provider (its own container).
        _redisProvider = new ServiceCollection()
            .AddLogging()
            .AddEFSecondLevelCache(options => options
                .UseStackExchangeRedisCacheProvider(RedisConnection, TimeSpan.FromMinutes(10))
                .ConfigureLogging(false)
                .UseCacheKeyPrefix("EF_"))
            .BuildServiceProvider();
    }

    private AppDbContext NewCachedContext(ServiceProvider provider)
    {
        var interceptor = provider.GetRequiredService<SecondLevelCacheInterceptor>();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .AddInterceptors(interceptor)
            .Options;
        return new AppDbContext(options);
    }

    private static int RunQuery(AppDbContext context) =>
        context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Take(100)
            .ToList()
            .Count;

    private static int RunCachedQuery(AppDbContext context) =>
        context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Take(100)
            .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(10))
            .ToList()
            .Count;

    // Baseline: every call is a fresh DbContext and a real database round trip.
    [Benchmark(Baseline = true)]
    public int Uncached()
    {
        using var context = new AppDbContext(_noCacheOptions);
        return RunQuery(context);
    }

    // Warm in-memory second-level cache: served from process memory, no DB hit.
    [Benchmark]
    public int CachedInMemory()
    {
        using var context = NewCachedContext(_memoryProvider);
        return RunCachedQuery(context);
    }

    // Warm Redis second-level cache: served from Redis over the network, no DB hit.
    [Benchmark]
    public int CachedRedis()
    {
        using var context = NewCachedContext(_redisProvider);
        return RunCachedQuery(context);
    }
}
