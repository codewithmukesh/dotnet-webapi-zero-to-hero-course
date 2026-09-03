using EfCoreContainsLargeList.Shared;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// AppDbContext configures its own provider in OnConfiguring so the same class can be
// reused by the probe and the benchmarks, so it is registered directly rather than
// through AddDbContext's options pipeline.
builder.Services.AddScoped(_ => new AppDbContext());

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// The default. Correct for the overwhelming majority of endpoints - EF Core 10 sends
// multiple scalar parameters, and switches to a single JSON parameter on its own once
// the list passes the 2,098 parameter ceiling.
app.MapPost("/products/by-ids", async (List<int> ids, AppDbContext db) =>
{
    var products = await db.Products
        .AsNoTracking()
        .Where(p => ids.Contains(p.Id))
        .ToListAsync();

    return Results.Ok(products);
});

// A short, stable set of values - the case EF.Constant is built for. Inlining lets the
// planner see the real values, and the SQL stays cacheable because the set rarely changes.
app.MapGet("/products/by-category-group", async (AppDbContext db) =>
{
    int[] featuredCategories = [1, 7, 23];

    var products = await db.Products
        .AsNoTracking()
        .Where(p => EF.Constant(featuredCategories).Contains(p.CategoryId))
        .ToListAsync();

    return Results.Ok(products);
});

// A large list on a hot path. EF.Parameter pins the single-JSON-parameter translation so
// the SQL shape stays identical no matter how many ids arrive, which keeps one plan in
// cache instead of one per distinct list length.
app.MapPost("/products/by-ids/bulk", async (List<int> ids, AppDbContext db) =>
{
    var products = await db.Products
        .AsNoTracking()
        .Where(p => EF.Parameter(ids).Contains(p.Id))
        .Select(p => new { p.Id, p.Sku, p.Name })
        .ToListAsync();

    return Results.Ok(products);
});

await DbSetup.EnsureSeededAsync();
app.Run();
