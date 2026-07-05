using EFCore.BulkExtensions;
using EfCoreBulkInsert.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.MaxBatchSize(1000)));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Native path: AddRange + a single SaveChanges. Right for up to a few thousand rows.
app.MapPost("/products/import", async (
    List<CreateProductRequest> requests,
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = requests.Select(r => new Product
    {
        Name = r.Name,
        Sku = r.Sku,
        Price = r.Price,
        Category = r.Category,
        Description = r.Description
    }).ToList();

    context.Products.AddRange(products);
    await context.SaveChangesAsync(ct);

    return Results.Ok(new { Inserted = products.Count });
});

// Bulk-library path: BulkInsert uses PostgreSQL binary COPY under the hood.
app.MapPost("/products/import/bulk", async (
    List<CreateProductRequest> requests,
    AppDbContext context,
    CancellationToken ct) =>
{
    var products = requests.Select(r => new Product
    {
        Name = r.Name,
        Sku = r.Sku,
        Price = r.Price,
        Category = r.Category,
        Description = r.Description
    }).ToList();

    await context.BulkInsertAsync(products, cancellationToken: ct);

    return Results.Ok(new { Inserted = products.Count });
});

// Fastest path: raw Npgsql binary COPY, no EF Core in the loop.
app.MapPost("/products/import/copy", async (
    List<CreateProductRequest> requests,
    CancellationToken ct) =>
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync(ct);

    await using var writer = await connection.BeginBinaryImportAsync(
        "COPY \"Products\" (\"Name\", \"Sku\", \"Price\", \"Category\", \"Description\", \"IsActive\", \"CreatedAtUtc\") FROM STDIN (FORMAT BINARY)",
        ct);

    var now = DateTime.UtcNow;
    foreach (var r in requests)
    {
        await writer.StartRowAsync(ct);
        await writer.WriteAsync(r.Name, NpgsqlDbType.Varchar, ct);
        await writer.WriteAsync(r.Sku, NpgsqlDbType.Varchar, ct);
        await writer.WriteAsync(r.Price, NpgsqlDbType.Numeric, ct);
        await writer.WriteAsync(r.Category, NpgsqlDbType.Varchar, ct);
        if (r.Description is null)
        {
            await writer.WriteNullAsync(ct);
        }
        else
        {
            await writer.WriteAsync(r.Description, NpgsqlDbType.Varchar, ct);
        }
        await writer.WriteAsync(true, NpgsqlDbType.Boolean, ct);
        await writer.WriteAsync(now, NpgsqlDbType.TimestampTz, ct);
    }

    await writer.CompleteAsync(ct);

    return Results.Ok(new { Inserted = requests.Count });
});

app.Run();

public record CreateProductRequest(
    string Name,
    string Sku,
    decimal Price,
    string Category,
    string? Description);
