using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApis.ProductCatalog.Api.Filters;
using MinimalApis.ProductCatalog.Api.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddValidation();

var app = builder.Build();

// Configure middleware
app.MapOpenApi();
app.MapOpenApi("/openapi/{documentName}.yaml");
app.MapScalarApiReference();

// Health check
app.MapGet("/", () => "Product Catalog API is running!")
    .ExcludeFromDescription();

// In-memory data store
var products = new List<Product>
{
    new(1, "Mechanical Keyboard", "Electronics", 149.99m, 50),
    new(2, "Wireless Mouse", "Electronics", 49.99m, 100),
    new(3, "USB-C Hub", "Accessories", 79.99m, 75),
    new(4, "Monitor Stand", "Accessories", 89.99m, 30),
    new(5, "Webcam HD", "Electronics", 129.99m, 45)
};
var nextId = 6;

// Route group for products
var productsGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .AddEndpointFilter<LoggingFilter>();

// GET all products with filtering and pagination
productsGroup.MapGet("/", Results<Ok<List<Product>>, NoContent> (
    string? category,
    decimal? minPrice,
    decimal? maxPrice,
    int page = 1,
    int pageSize = 10) =>
{
    var query = products.AsEnumerable();

    if (!string.IsNullOrEmpty(category))
        query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice);

    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice);

    var result = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return result.Count > 0 ? TypedResults.Ok(result) : TypedResults.NoContent();
})
.WithName("GetProducts")
.WithSummary("Gets all products")
.WithDescription("Returns a paginated list of products with optional filtering by category and price range.");

// GET single product by ID
productsGroup.MapGet("/{id:int}", Results<Ok<Product>, NotFound> (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is not null ? TypedResults.Ok(product) : TypedResults.NotFound();
})
.WithName("GetProductById")
.WithSummary("Gets a product by ID")
.WithDescription("Returns a single product based on the provided ID. Returns 404 if not found.");

// GET products by category
productsGroup.MapGet("/category/{category}", Results<Ok<List<Product>>, NotFound> (string category) =>
{
    var result = products
        .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
        .ToList();

    return result.Count > 0 ? TypedResults.Ok(result) : TypedResults.NotFound();
})
.WithName("GetProductsByCategory")
.WithSummary("Gets products by category")
.WithDescription("Returns all products in the specified category.");

// POST create new product
productsGroup.MapPost("/", Results<Created<Product>, BadRequest<string>> (CreateProductRequest request) =>
{
    var product = new Product(
        nextId++,
        request.Name,
        request.Category,
        request.Price,
        request.Stock
    );

    products.Add(product);
    return TypedResults.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithSummary("Creates a new product")
.WithDescription("Creates a new product with the provided details. Returns the created product with its generated ID.");

// PUT update existing product
productsGroup.MapPut("/{id:int}", Results<NoContent, NotFound, BadRequest<string>> (int id, UpdateProductRequest request) =>
{
    var index = products.FindIndex(p => p.Id == id);
    if (index == -1) return TypedResults.NotFound();

    products[index] = new Product(
        id,
        request.Name,
        request.Category,
        request.Price,
        request.Stock
    );

    return TypedResults.NoContent();
})
.WithName("UpdateProduct")
.WithSummary("Updates an existing product")
.WithDescription("Updates all properties of an existing product. Returns 404 if the product doesn't exist.");

// PATCH update product stock
productsGroup.MapPatch("/{id:int}/stock", Results<Ok<Product>, NotFound, BadRequest<string>> (
    int id,
    UpdateStockRequest request) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null) return TypedResults.NotFound();

    var updatedProduct = product with { Stock = request.Stock };
    var index = products.FindIndex(p => p.Id == id);
    products[index] = updatedProduct;

    return TypedResults.Ok(updatedProduct);
})
.WithName("UpdateProductStock")
.WithSummary("Updates product stock")
.WithDescription("Updates only the stock quantity of an existing product.");

// DELETE product
productsGroup.MapDelete("/{id:int}", Results<NoContent, NotFound> (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null) return TypedResults.NotFound();

    products.Remove(product);
    return TypedResults.NoContent();
})
.WithName("DeleteProduct")
.WithSummary("Deletes a product")
.WithDescription("Permanently removes a product from the catalog. Returns 404 if the product doesn't exist.");

// GET product statistics
productsGroup.MapGet("/stats", (ILogger<Program> logger) =>
{
    var stats = new
    {
        TotalProducts = products.Count,
        TotalStock = products.Sum(p => p.Stock),
        AveragePrice = products.Average(p => p.Price),
        Categories = products.GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToList()
    };

    logger.LogInformation("Statistics requested: {TotalProducts} products", stats.TotalProducts);
    return TypedResults.Ok(stats);
})
.WithName("GetProductStats")
.WithSummary("Gets product statistics")
.WithDescription("Returns aggregate statistics about the product catalog including total count, stock, and category breakdown.");

app.Run();
