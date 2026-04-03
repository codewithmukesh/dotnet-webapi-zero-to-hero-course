using Mediator;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Api.Data;

namespace VerticalSlice.Api.Features.Products;

public static class GetProducts
{
    public record Query : IRequest<List<Response>>;

    public record Response(Guid Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt);

    internal sealed class Handler(AppDbContext context) : IRequestHandler<Query, List<Response>>
    {
        public async ValueTask<List<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await context.Products
                .AsNoTracking()
                .Select(p => new Response(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new Query(), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<List<Response>>();
    }
}
