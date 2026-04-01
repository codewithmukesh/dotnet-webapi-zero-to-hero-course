using Mediator;
using VerticalSlice.Api.Data;

namespace VerticalSlice.Api.Features.Products;

public static class GetProductById
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(Guid Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt);

    internal sealed class Handler(AppDbContext context) : IRequestHandler<Query, Response?>
    {
        public async ValueTask<Response?> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([request.Id], cancellationToken);

            if (product is null)
            {
                return null;
            }

            return new Response(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.CreatedAt);
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new Query(id), cancellationToken);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetProductById")
        .WithTags("Products")
        .Produces<Response>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
