using HybridCaching.Api.Data;
using HybridCaching.Api.Models;
using HybridCaching.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

// Add HybridCache with Redis as L2 backend
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(5),
        Expiration = TimeSpan.FromMinutes(30)
    };
    options.MaximumPayloadBytes = 1024 * 1024; // 1 MB max per entry
});
#pragma warning restore EXTEXP0018

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var products = app.MapGroup("/products").WithTags("Products");

products.MapGet("/", async (IProductService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetAllAsync(cancellationToken);
    return TypedResults.Ok(result);
})
.WithName("GetAllProducts")
.WithSummary("Get all products")
.WithDescription("Returns all products from the database or HybridCache.");

products.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetByIdAsync(id, cancellationToken);
    return product is not null
        ? TypedResults.Ok(product)
        : Results.NotFound();
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Returns a single product from the database or HybridCache.");

products.MapGet("/category/{category}", async (string category, IProductService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetByCategoryAsync(category, cancellationToken);
    return TypedResults.Ok(result);
})
.WithName("GetProductsByCategory")
.WithSummary("Get products by category")
.WithDescription("Returns products filtered by category, demonstrating tag-based caching.");

products.MapPost("/", async (ProductCreationDto request, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.CreateAsync(request, cancellationToken);
    return TypedResults.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithSummary("Create a new product")
.WithDescription("Creates a new product and invalidates all product-related cache entries via tag-based invalidation.");

products.MapPut("/{id:guid}", async (Guid id, ProductCreationDto request, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.UpdateAsync(id, request, cancellationToken);
    return product is not null
        ? TypedResults.Ok(product)
        : Results.NotFound();
})
.WithName("UpdateProduct")
.WithSummary("Update a product")
.WithDescription("Updates a product and invalidates the individual product cache and all list caches.");

products.MapDelete("/{id:guid}", async (Guid id, IProductService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(id, cancellationToken);
    return deleted
        ? TypedResults.NoContent()
        : Results.NotFound();
})
.WithName("DeleteProduct")
.WithSummary("Delete a product")
.WithDescription("Deletes a product and invalidates the individual product cache and all list caches.");

app.Run();
