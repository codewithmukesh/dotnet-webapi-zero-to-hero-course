using Hybrid.Api.Shared;
using Hybrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hybrid.Api.Features.Products;

public static class ChangeProductPrice
{
    public sealed record Request(decimal Price);

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPut("/api/products/{id:guid}/price", Handle)
                .WithName(nameof(ChangeProductPrice))
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

        product.ChangePrice(request.Price);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
