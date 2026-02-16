using ConcurrencyControl.Api.Data;
using ConcurrencyControl.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Seed test data
app.MapPost("/products/seed", async (
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = new List<Product>
    {
        new() { Name = "Wireless Mouse", Price = 29.99m, Stock = 150, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new() { Name = "Mechanical Keyboard", Price = 89.99m, Stock = 75, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new() { Name = "USB-C Hub", Price = 49.99m, Stock = 200, Category = "Electronics", CreatedAt = DateTime.UtcNow },
        new() { Name = "Monitor Stand", Price = 39.99m, Stock = 50, Category = "Accessories", CreatedAt = DateTime.UtcNow },
        new() { Name = "Desk Lamp", Price = 24.99m, Stock = 100, Category = "Accessories", CreatedAt = DateTime.UtcNow }
    };

    context.Products.AddRange(products);
    await context.SaveChangesAsync(ct);

    return Results.Created("/products", new { Count = products.Count });
});

// GET all products
app.MapGet("/products", async (
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = await context.Products
        .AsNoTracking()
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.Stock,
            p.Category,
            p.RowVersion
        })
        .ToListAsync(ct);

    return Results.Ok(products);
});

// GET single product
app.MapGet("/products/{id:int}", async (
    int id,
    AppDbContext context,
    CancellationToken ct) =>
{
    var product = await context.Products
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.Stock,
            p.Category,
            p.RowVersion
        })
        .FirstOrDefaultAsync(ct);

    return product is null
        ? Results.NotFound(new { Error = $"Product with ID {id} not found." })
        : Results.Ok(product);
});

// UPDATE product — optimistic concurrency with RowVersion
app.MapPut("/products/{id:int}", async (
    int id,
    UpdateProductRequest request,
    AppDbContext context,
    CancellationToken ct) =>
{
    var product = await context.Products.FindAsync([id], ct);
    if (product is null)
        return Results.NotFound(new { Error = $"Product with ID {id} not found." });

    // Set the original RowVersion so EF Core includes it in the WHERE clause
    context.Entry(product).Property(p => p.RowVersion).OriginalValue = request.RowVersion;

    product.Name = request.Name;
    product.Price = request.Price;
    product.Stock = request.Stock;
    product.Category = request.Category;
    product.LastModified = DateTime.UtcNow;

    try
    {
        await context.SaveChangesAsync(ct);
        return Results.Ok(new
        {
            product.Id,
            product.Name,
            product.Price,
            product.Stock,
            product.Category,
            product.RowVersion
        });
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(new
        {
            Error = "This product was modified by another user. Please refresh and try again.",
            CurrentVersion = (await context.Products.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new { p.RowVersion, p.Name, p.Price, p.Stock })
                .FirstOrDefaultAsync(ct))
        });
    }
});

// UPDATE stock only — optimistic concurrency (common scenario: inventory management)
app.MapPatch("/products/{id:int}/stock", async (
    int id,
    UpdateStockRequest request,
    AppDbContext context,
    CancellationToken ct) =>
{
    var product = await context.Products.FindAsync([id], ct);
    if (product is null)
        return Results.NotFound(new { Error = $"Product with ID {id} not found." });

    context.Entry(product).Property(p => p.RowVersion).OriginalValue = request.RowVersion;

    product.Stock = request.NewStock;
    product.LastModified = DateTime.UtcNow;

    try
    {
        await context.SaveChangesAsync(ct);
        return Results.Ok(new { product.Id, product.Stock, product.RowVersion });
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(new
        {
            Error = "Stock was modified by another operation. Please refresh and retry.",
            CurrentStock = await context.Products.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new { p.Stock, p.RowVersion })
                .FirstOrDefaultAsync(ct)
        });
    }
});

// UPDATE with retry — automatic conflict resolution (last-write-wins)
app.MapPatch("/products/{id:int}/stock-with-retry", async (
    int id,
    StockAdjustmentRequest request,
    AppDbContext context,
    CancellationToken ct) =>
{
    const int maxRetries = 3;

    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        var product = await context.Products.FindAsync([id], ct);
        if (product is null)
            return Results.NotFound(new { Error = $"Product with ID {id} not found." });

        product.Stock += request.Adjustment;
        if (product.Stock < 0)
            return Results.BadRequest(new { Error = "Insufficient stock." });

        product.LastModified = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                product.Id,
                product.Stock,
                product.RowVersion,
                Attempt = attempt + 1
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            // Detach the entity so we can re-read fresh data
            context.Entry(product).State = EntityState.Detached;
        }
    }

    return Results.Conflict(new
    {
        Error = $"Failed to update stock after {maxRetries} attempts due to concurrent modifications."
    });
});

// Simulate concurrent requests — demonstrates the conflict
app.MapPost("/products/{id:int}/simulate-conflict", async (
    int id,
    AppDbContext context,
    IServiceProvider sp,
    CancellationToken ct) =>
{
    var product = await context.Products.AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id, ct);

    if (product is null)
        return Results.NotFound(new { Error = $"Product with ID {id} not found." });

    var results = new List<object>();

    // Fire 5 concurrent updates to the same product
    var tasks = Enumerable.Range(1, 5).Select(async i =>
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var p = await db.Products.FindAsync([id], ct);
        if (p is null) return new { Task = i, Status = "NotFound", Detail = "Product not found" };

        p.Price = product.Price + (i * 1.00m);
        p.LastModified = DateTime.UtcNow;

        // Add a small delay to increase the chance of conflicts
        await Task.Delay(10, ct);

        try
        {
            await db.SaveChangesAsync(ct);
            return new { Task = i, Status = "Success", Detail = $"Price updated to {p.Price}" };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new { Task = i, Status = "Conflict", Detail = "DbUpdateConcurrencyException caught" };
        }
    });

    var taskResults = await Task.WhenAll(tasks);

    return Results.Ok(new
    {
        Message = "Concurrent update simulation complete",
        Successes = taskResults.Count(r => r.Status == "Success"),
        Conflicts = taskResults.Count(r => r.Status == "Conflict"),
        Details = taskResults
    });
});

app.Run();

public record UpdateProductRequest(string Name, decimal Price, int Stock, string Category, uint RowVersion);
public record UpdateStockRequest(int NewStock, uint RowVersion);
public record StockAdjustmentRequest(int Adjustment);
