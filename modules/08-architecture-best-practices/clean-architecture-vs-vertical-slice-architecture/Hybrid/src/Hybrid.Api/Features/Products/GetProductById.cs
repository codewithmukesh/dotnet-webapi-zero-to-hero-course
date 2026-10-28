using Hybrid.Api.Shared;
using Hybrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Hybrid.Api.Features.Products;

public static class GetProductById
{
    public sealed record Response(Guid Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt);

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet("/api/products/{id:guid}", Handle)
                .WithName(nameof(GetProductById))
                .WithTags("Products");
    }

    internal static async Task<Results<Ok<Response>, NotFound>> Handle(Guid id, AppDbContext db, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new Response(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
    }
}
