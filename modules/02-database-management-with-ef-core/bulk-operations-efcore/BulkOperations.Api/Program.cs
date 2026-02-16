using BulkOperations.Api.Data;
using BulkOperations.Api.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MaxBatchSize(100)));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Seed test data
app.MapPost("/products/seed/{count:int}", async (
    int count,
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = Enumerable.Range(1, count).Select(i => new Product
    {
        Name = $"Product {i}",
        Price = Math.Round((decimal)(Random.Shared.NextDouble() * 1000), 2),
        Category = i % 3 == 0 ? "Electronics" : i % 3 == 1 ? "Books" : "Clothing",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    }).ToList();

    context.Products.AddRange(products);
    await context.SaveChangesAsync(ct);

    return Results.Created($"/products", new { Count = products.Count });
});

// Bulk insert with AddRange
app.MapPost("/products/bulk", async (
    List<CreateProductRequest> requests,
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = requests.Select(r => new Product
    {
        Name = r.Name,
        Price = r.Price,
        Category = r.Category,
        CreatedAt = DateTime.UtcNow
    }).ToList();

    context.Products.AddRange(products);
    await context.SaveChangesAsync(ct);

    return Results.Created($"/products", new { Count = products.Count });
});

// Bulk insert with EFCore.BulkExtensions
app.MapPost("/products/bulk-fast", async (
    List<CreateProductRequest> requests,
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = requests.Select(r => new Product
    {
        Name = r.Name,
        Price = r.Price,
        Category = r.Category,
        CreatedAt = DateTime.UtcNow
    }).ToList();

    await context.BulkInsertAsync(products, cancellationToken: ct);

    return Results.Created($"/products", new { Count = products.Count });
});

// Bulk update with ExecuteUpdate — apply price multiplier by category
app.MapPut("/products/bulk-update", async (
    BulkUpdateRequest request,
    AppDbContext context,
    CancellationToken ct) =>
{
    var affected = await context.Products
        .Where(p => p.Category == request.Category)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.Price, p => p.Price * request.PriceMultiplier)
            .SetProperty(p => p.LastModified, DateTime.UtcNow), ct);

    return Results.Ok(new { AffectedRows = affected });
});

// Bulk deactivate — deactivate products older than N days
app.MapPut("/products/bulk-deactivate", async (
    int olderThanDays,
    AppDbContext context,
    CancellationToken ct) =>
{
    var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
    var affected = await context.Products
        .Where(p => p.CreatedAt < cutoff && p.IsActive)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.IsActive, false)
            .SetProperty(p => p.LastModified, DateTime.UtcNow), ct);

    return Results.Ok(new { DeactivatedCount = affected });
});

// Bulk hard delete with ExecuteDelete
app.MapDelete("/products/bulk-delete/{category}", async (
    string category,
    AppDbContext context,
    CancellationToken ct) =>
{
    var deleted = await context.Products
        .Where(p => p.Category == category)
        .ExecuteDeleteAsync(ct);

    return Results.Ok(new { DeletedCount = deleted });
});

// Bulk soft delete via ExecuteUpdate (safe alternative)
app.MapDelete("/products/bulk-soft-delete/{category}", async (
    string category,
    AppDbContext context,
    CancellationToken ct) =>
{
    var affected = await context.Products
        .Where(p => p.Category == category)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.IsDeleted, true)
            .SetProperty(p => p.LastModified, DateTime.UtcNow), ct);

    return Results.Ok(new { SoftDeletedCount = affected });
});

// Transactional bulk operations — deactivate + clean up in one atomic operation
app.MapPost("/products/bulk-cleanup", async (
    AppDbContext context,
    CancellationToken ct) =>
{
    await using var transaction = await context.Database.BeginTransactionAsync(ct);

    try
    {
        var deactivated = await context.Products
            .Where(p => p.CreatedAt < DateTime.UtcNow.AddDays(-90) && p.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.IsActive, false)
                .SetProperty(p => p.LastModified, DateTime.UtcNow), ct);

        var softDeleted = await context.Products
            .Where(p => !p.IsActive && p.CreatedAt < DateTime.UtcNow.AddDays(-180))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.IsDeleted, true)
                .SetProperty(p => p.LastModified, DateTime.UtcNow), ct);

        await transaction.CommitAsync(ct);

        return Results.Ok(new { Deactivated = deactivated, SoftDeleted = softDeleted });
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
});

app.Run();

public record CreateProductRequest(string Name, decimal Price, string Category);
public record BulkUpdateRequest(string Category, decimal PriceMultiplier);
