using Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapPost("/", async (
            CreateProductRequest request,
            IValidator<CreateProductRequest> validator,
            AppDbContext db,
            ProductMapper mapper,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var product = new Product
            {
                Id = Guid.CreateVersion7(),
                Name = request.Name,
                Sku = request.Sku,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            db.Products.Add(product);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/products/{product.Id}", mapper.ToDto(product));
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            HybridCache cache,
            AppDbContext db,
            ProductMapper mapper,
            CancellationToken ct) =>
        {
            var product = await cache.GetOrCreateAsync(
                $"product:{id}",
                async token =>
                {
                    var entity = await db.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id, token);

                    return entity is null ? null : mapper.ToDto(entity);
                },
                cancellationToken: ct);

            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        return app;
    }
}
