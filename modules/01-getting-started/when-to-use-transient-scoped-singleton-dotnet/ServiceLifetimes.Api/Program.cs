using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ServiceLifetimes.Api.Caching;
using ServiceLifetimes.Api.Data;
using ServiceLifetimes.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// The three lifetimes
builder.Services.AddTransient<ProductValidator>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("products")); // scoped by default
builder.Services.AddSingleton<ProductCache>();

// Two classes that both need all three services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PricingService>();

// The captive dependency, off by default. In Development, ASP.NET Core validates scopes
// at startup and refuses to build the app with this registration.
var captiveEnabled = builder.Configuration.GetValue<bool>("Demo:EnableCaptiveDependency");
if (captiveEnabled)
{
    builder.Services.AddSingleton<CaptiveProductCache>();
}

// Optional: run the same scope validation in every environment, not only Development.
if (builder.Configuration.GetValue<bool>("Demo:ValidateScopesEverywhere"))
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

var app = builder.Build();

// Seed the in-memory database. There is no request here, so create a scope by hand.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.MapOpenApi();
app.MapScalarApiReference();

// 1. See the lifetimes: two classes, one request, three services each.
app.MapGet("/lifetimes", (ProductService products, PricingService pricing) =>
    new { productService = products.Describe(), pricingService = pricing.Describe() });

// 2. The fix: a singleton cache that creates a fresh scope whenever it needs the database.
app.MapGet("/fixed/products", async (ProductCache cache, bool? refresh, CancellationToken cancellationToken) =>
{
    var items = await cache.GetProductsAsync(refresh ?? false, cancellationToken);
    return new { cacheInstance = cache.InstanceId.Short(), loadedWithDbContext = cache.LastDbContextId?.Short(), products = items.Count };
});

// 3. The bug: only mapped when the captive dependency is switched on.
if (captiveEnabled)
{
    app.MapGet("/captive/products", async (CaptiveProductCache cache, AppDbContext requestDb, CancellationToken cancellationToken) =>
    {
        var items = await cache.GetProductsAsync(cancellationToken);
        return new { thisRequestsDbContext = requestDb.ContextId.InstanceId.Short(), cachesDbContext = cache.DbContextId.Short(), products = items.Count };
    });
}

app.Run();
