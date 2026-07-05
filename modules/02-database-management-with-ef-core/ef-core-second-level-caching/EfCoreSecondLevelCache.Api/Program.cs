using EFCoreSecondLevelCacheInterceptor;
using EfCoreSecondLevelCache.Shared;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// 1. Register the second-level cache with the in-memory provider (v5: the
//    provider lives in the separate EFCoreSecondLevelCacheInterceptor.MemoryCache package).
builder.Services.AddEFSecondLevelCache(options =>
    options
        .UseMemoryCacheProvider()
        .ConfigureLogging(true)
        .UseCacheKeyPrefix("EF_")
        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1)));

// 2. Wire the interceptor into the DbContext through the service provider.
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(connectionString)
        .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Cached read: the first call hits the database, every later call is served
// from the second-level cache until it expires or a SaveChanges invalidates it.
app.MapGet("/products", async (AppDbContext context, CancellationToken ct) =>
    await context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Take(100)
        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5))
        .ToListAsync(ct));

// Uncached read for comparison: always hits the database.
app.MapGet("/products/uncached", async (AppDbContext context, CancellationToken ct) =>
    await context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Take(100)
        .ToListAsync(ct));

// Write through SaveChanges: the interceptor sees the modified table and
// invalidates the cached Products queries automatically.
app.MapPost("/products", async (
    CreateProductRequest request, AppDbContext context, CancellationToken ct) =>
{
    var product = new Product
    {
        Name = request.Name,
        Category = request.Category,
        Price = request.Price
    };
    context.Products.Add(product);
    await context.SaveChangesAsync(ct); // auto-invalidates the Products cache
    return Results.Created($"/products/{product.Id}", product);
});

// The trap: ExecuteUpdate bypasses the change tracker, so the interceptor does
// NOT auto-invalidate. You must clear the cache yourself, or reads stay stale.
app.MapPost("/products/deactivate-old", async (
    AppDbContext context,
    IEFCacheServiceProvider cache,
    CancellationToken ct) =>
{
    var affected = await context.Products
        .Where(p => p.CreatedAtUtc < DateTime.UtcNow.AddYears(-1))
        .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false), ct);

    // Without this line the cached /products response keeps serving the
    // now-deactivated rows until the 5-minute TTL expires.
    cache.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        nameof(AppDbContext.Products)
    }));

    return Results.Ok(new { Deactivated = affected });
});

app.Run();

public record CreateProductRequest(string Name, string Category, decimal Price);
