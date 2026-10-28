using Hybrid.Api.Shared;
using Hybrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hybrid.Api.Features.Products;

public static class RemoveStock
{
    public sealed record Request(int Quantity);

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost("/api/products/{id:guid}/remove-stock", Handle)
                .WithName(nameof(RemoveStock))
                .WithTags("Products");
    }

    internal static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        Request request,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return TypedResults.NotFound();
        }

        product.RemoveStock(request.Quantity);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
