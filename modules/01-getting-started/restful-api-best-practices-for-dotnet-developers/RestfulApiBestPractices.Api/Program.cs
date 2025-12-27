using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Middleware
app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference();

// API Routes - Version 1
var api = app.MapGroup("/api/v1");

// GET /api/v1/products - Get all products (paginated)
api.MapGet("/products", async (
    int page = 1,
    int pageSize = 20,
    string? category = null,
    string? sort = "id",
    string order = "asc",
    IProductService service = default!) =>
{
    pageSize = Math.Clamp(pageSize, 1, 100);
    var (products, totalCount) = await service.GetPagedAsync(page, pageSize, category, sort, order == "desc");
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    return Results.Ok(new PagedResponse<ProductResponse>(
        products.Select(p => p.ToResponse()),
        new PaginationMeta(page, pageSize, totalPages, totalCount, page < totalPages, page > 1)
    ));
})
.WithName("GetProducts")
.WithSummary("Get all products with pagination, filtering, and sorting")
.WithDescription("Returns a paginated list of products. Supports filtering by category and sorting by id, name, price, or createdAt.")
.Produces<PagedResponse<ProductResponse>>(StatusCodes.Status200OK)
.WithTags("Products");

// GET /api/v1/products/{id} - Get a product by ID
api.MapGet("/products/{id:int}", async (int id, IProductService service) =>
{
    var product = await service.GetByIdAsync(id);
    return product is null
        ? Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        })
        : Results.Ok(product.ToResponse());
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Returns a single product based on its unique identifier. Returns 404 if the product doesn't exist.")
.Produces<ProductResponse>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// POST /api/v1/products - Create a new product
api.MapPost("/products", async (CreateProductRequest request, IProductService service) =>
{
    // Basic validation
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(request.Name))
        errors["name"] = ["Name is required."];
    if (request.Name?.Length > 200)
        errors["name"] = ["Name must be 200 characters or less."];
    if (string.IsNullOrWhiteSpace(request.Description))
        errors["description"] = ["Description is required."];
    if (request.Price <= 0)
        errors["price"] = ["Price must be greater than 0."];
    if (request.Stock < 0)
        errors["stock"] = ["Stock cannot be negative."];

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var product = await service.CreateAsync(request);
    return Results.Created($"/api/v1/products/{product.Id}", product.ToResponse());
})
.WithName("CreateProduct")
.WithSummary("Create a new product")
.WithDescription("Creates a new product and returns it with a 201 Created status and Location header.")
.Produces<ProductResponse>(StatusCodes.Status201Created)
.Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
.WithTags("Products");

// PUT /api/v1/products/{id} - Update a product (full replacement)
api.MapPut("/products/{id:int}", async (int id, UpdateProductRequest request, IProductService service) =>
{
    var product = await service.UpdateAsync(id, request);
    return product is null
        ? Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        })
        : Results.NoContent();
})
.WithName("UpdateProduct")
.WithSummary("Update an existing product (full replacement)")
.WithDescription("Replaces all fields of an existing product. All fields must be provided.")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// PATCH /api/v1/products/{id} - Partially update a product
api.MapPatch("/products/{id:int}", async (int id, PatchProductRequest request, IProductService service) =>
{
    var product = await service.PatchAsync(id, request);
    return product is null
        ? Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        })
        : Results.NoContent();
})
.WithName("PatchProduct")
.WithSummary("Partially update a product")
.WithDescription("Updates only the provided fields of a product. Omitted fields remain unchanged.")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// DELETE /api/v1/products/{id} - Delete a product
api.MapDelete("/products/{id:int}", async (int id, IProductService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted
        ? Results.NoContent()
        : Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {id} was not found.",
            Instance = $"/api/v1/products/{id}"
        });
})
.WithName("DeleteProduct")
.WithSummary("Delete a product")
.WithDescription("Removes a product from the system. Returns 204 on success, 404 if not found.")
.Produces(StatusCodes.Status204NoContent)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.WithTags("Products");

// Nested resource example: GET /api/v1/products/{id}/reviews
api.MapGet("/products/{productId:int}/reviews", async (int productId, IProductService service) =>
{
    var product = await service.GetByIdAsync(productId);
    if (product is null)
    {
        return Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Product Not Found",
            Detail = $"Product with ID {productId} was not found.",
            Instance = $"/api/v1/products/{productId}/reviews"
        });
    }

    // Return mock reviews (in a real app, this would be a separate service)
    var reviews = new[]
    {
        new { Id = 1, ProductId = productId, Rating = 5, Comment = "Excellent product!", Author = "John Doe", CreatedAt = DateTime.UtcNow.AddDays(-10) },
        new { Id = 2, ProductId = productId, Rating = 4, Comment = "Good value for money.", Author = "Jane Smith", CreatedAt = DateTime.UtcNow.AddDays(-5) }
    };

    return Results.Ok(reviews);
})
.WithName("GetProductReviews")
.WithSummary("Get reviews for a product")
.WithDescription("Returns all reviews for a specific product. Demonstrates nested resource pattern.")
.WithTags("Products");

app.Run();

// ============================================================================
// Models
// ============================================================================

public record Product
{
    public int Id { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ProductResponse ToResponse() => new(
        Id, Name, Description, Price, Stock, Category, CreatedAt, UpdatedAt
    );
}

// ============================================================================
// DTOs (Data Transfer Objects)
// ============================================================================

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? Category
);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? Category
);

public record PatchProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? Stock = null,
    string? Category = null
);

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ============================================================================
// Pagination
// ============================================================================

public record PagedResponse<T>(
    IEnumerable<T> Data,
    PaginationMeta Pagination
);

public record PaginationMeta(
    int Page,
    int PageSize,
    int TotalPages,
    int TotalCount,
    bool HasNext,
    bool HasPrevious
);

// ============================================================================
// Service Layer
// ============================================================================

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? category = null, string? sortBy = "id", bool descending = false);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product?> UpdateAsync(int id, UpdateProductRequest request);
    Task<Product?> PatchAsync(int id, PatchProductRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private static readonly List<Product> _products =
    [
        new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop for developers", Price = 999.99m, Stock = 50, Category = "Electronics" },
        new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking", Price = 29.99m, Stock = 200, Category = "Electronics" },
        new Product { Id = 3, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard with Cherry MX switches", Price = 149.99m, Stock = 100, Category = "Electronics" },
        new Product { Id = 4, Name = "4K Monitor", Description = "27-inch 4K UHD monitor with HDR support", Price = 449.99m, Stock = 30, Category = "Electronics" },
        new Product { Id = 5, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI and SD card reader", Price = 49.99m, Stock = 150, Category = "Accessories" },
        new Product { Id = 6, Name = "Webcam HD", Description = "1080p HD webcam with built-in microphone", Price = 79.99m, Stock = 80, Category = "Electronics" },
        new Product { Id = 7, Name = "Noise-Cancelling Headphones", Description = "Premium wireless headphones with ANC", Price = 299.99m, Stock = 45, Category = "Audio" },
        new Product { Id = 8, Name = "Standing Desk", Description = "Electric height-adjustable standing desk", Price = 599.99m, Stock = 20, Category = "Furniture" },
        new Product { Id = 9, Name = "Ergonomic Chair", Description = "Mesh ergonomic office chair with lumbar support", Price = 399.99m, Stock = 35, Category = "Furniture" },
        new Product { Id = 10, Name = "Desk Lamp", Description = "LED desk lamp with adjustable color temperature", Price = 39.99m, Stock = 120, Category = "Accessories" }
    ];

    private static int _nextId = 11;
    private static readonly Lock _lock = new();

    public Task<IEnumerable<Product>> GetAllAsync()
        => Task.FromResult<IEnumerable<Product>>(_products);

    public Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? category = null, string? sortBy = "id", bool descending = false)
    {
        var query = _products.AsEnumerable();

        // Filter by category
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Sort
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "stock" => descending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            _ => descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };

        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize);

        return Task.FromResult((items, totalCount));
    }

    public Task<Product?> GetByIdAsync(int id)
        => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<Product> CreateAsync(CreateProductRequest request)
    {
        lock (_lock)
        {
            var product = new Product
            {
                Id = _nextId++,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                Category = request.Category
            };
            _products.Add(product);
            return Task.FromResult(product);
        }
    }

    public Task<Product?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult<Product?>(null);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Category = request.Category;
        product.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Product?>(product);
    }

    public Task<Product?> PatchAsync(int id, PatchProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult<Product?>(null);

        if (request.Name is not null) product.Name = request.Name;
        if (request.Description is not null) product.Description = request.Description;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.Stock.HasValue) product.Stock = request.Stock.Value;
        if (request.Category is not null) product.Category = request.Category;
        product.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Product?>(product);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult(false);

        _products.Remove(product);
        return Task.FromResult(true);
    }
}
