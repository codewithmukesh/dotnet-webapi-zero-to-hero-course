using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SoftDeletes.Api.Data;
using SoftDeletes.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<SoftDeleteInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>());
});

var app = builder.Build();

// Ensure database is created (for development only)
await using (var serviceScope = app.Services.CreateAsyncScope())
await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/categories", async (AppDbContext context, CancellationToken cancellationToken) =>
    Results.Ok(await context.Categories
        .Include(c => c.Products)
        .ToListAsync(cancellationToken)));

app.MapPost("/categories", async (string name, AppDbContext context, CancellationToken cancellationToken) =>
{
    var category = new Category { Name = name };
    context.Categories.Add(category);
    await context.SaveChangesAsync(cancellationToken);
    return Results.Created($"/categories/{category.Id}", category);
});

app.MapDelete("/categories/{id:guid}", async (Guid id, AppDbContext context, CancellationToken cancellationToken) =>
{
    var category = await context.Categories
        .Include(c => c.Products) // Load children for cascade soft delete
        .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    if (category is null) return Results.NotFound();

    context.Categories.Remove(category);
    await context.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
});

app.MapPost("/categories/{id:guid}/restore", async (Guid id, AppDbContext context, CancellationToken cancellationToken) =>
{
    var category = await context.Categories
        .IgnoreQueryFilters(["SoftDelete"])
        .Include(c => c.Products.Where(p => p.IsDeleted))
        .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    if (category is null) return Results.NotFound();
    if (!category.IsDeleted) return Results.BadRequest("Category is not deleted.");

    category.IsDeleted = false;
    category.DeletedOnUtc = null;
    category.DeletedBy = null;

    // Restore cascade-deleted children
    foreach (var product in category.Products)
    {
        product.IsDeleted = false;
        product.DeletedOnUtc = null;
        product.DeletedBy = null;
    }

    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok(category);
});

app.MapGet("/categories/deleted", async (AppDbContext context, CancellationToken cancellationToken) =>
    Results.Ok(await context.Categories
        .IgnoreQueryFilters(["SoftDelete"])
        .Where(c => c.IsDeleted)
        .OrderByDescending(c => c.DeletedOnUtc)
        .ToListAsync(cancellationToken)));

app.Run();
