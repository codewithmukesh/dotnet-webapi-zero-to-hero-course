using EfCoreInterceptors.Api.Data;
using EfCoreInterceptors.Api.Entities;
using EfCoreInterceptors.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Needed so CurrentUser can read the request (the claim or the demo header).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// The audit interceptor depends on a scoped service, so it must be scoped too
// and resolved through the AddDbContext((sp, options) => ...) overload below.
builder.Services.AddScoped<AuditableInterceptor>();

// The slow-query interceptor is stateless, so a singleton is fine.
builder.Services.AddSingleton<SlowQueryInterceptor>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(
        serviceProvider.GetRequiredService<AuditableInterceptor>(),
        serviceProvider.GetRequiredService<SlowQueryInterceptor>());
});

var app = builder.Build();

// Ensure the database is created (development only).
await using (var scope = app.Services.CreateAsyncScope())
await using (var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// CreatedAtUtc / CreatedBy are stamped by the interceptor, never set here.
app.MapPost("/products", async (CreateProductRequest request, AppDbContext context, CancellationToken ct) =>
{
    var product = new Product { Name = request.Name, Price = request.Price };
    context.Products.Add(product);
    await context.SaveChangesAsync(ct);
    return Results.Created($"/products/{product.Id}", product);
});

app.MapGet("/products", async (AppDbContext context, CancellationToken ct) =>
    // Soft-deleted rows are hidden by the global query filter automatically.
    Results.Ok(await context.Products.ToListAsync(ct)));

// UpdatedAtUtc / UpdatedBy get stamped because the interceptor calls DetectChanges.
app.MapPut("/products/{id:int}", async (int id, UpdateProductRequest request, AppDbContext context, CancellationToken ct) =>
{
    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    if (product is null) return Results.NotFound();

    product.Name = request.Name;
    product.Price = request.Price;
    await context.SaveChangesAsync(ct);
    return Results.Ok(product);
});

// A "delete" becomes a soft delete inside the interceptor.
app.MapDelete("/products/{id:int}", async (int id, AppDbContext context, CancellationToken ct) =>
{
    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    if (product is null) return Results.NotFound();

    context.Products.Remove(product);
    await context.SaveChangesAsync(ct);
    return Results.NoContent();
});

// Bypass the query filter to see the rows that were soft-deleted.
app.MapGet("/products/deleted", async (AppDbContext context, CancellationToken ct) =>
    Results.Ok(await context.Products
        .IgnoreQueryFilters()
        .Where(p => p.IsDeleted)
        .OrderByDescending(p => p.DeletedAtUtc)
        .ToListAsync(ct)));

app.Run();

internal record CreateProductRequest(string Name, decimal Price);
internal record UpdateProductRequest(string Name, decimal Price);
