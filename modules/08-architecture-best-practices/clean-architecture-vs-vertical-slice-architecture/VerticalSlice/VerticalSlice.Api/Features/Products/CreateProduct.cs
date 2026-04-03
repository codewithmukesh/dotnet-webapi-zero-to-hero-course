using Mediator;
using VerticalSlice.Api.Data;
using VerticalSlice.Api.Entities;

namespace VerticalSlice.Api.Features.Products;

public static class CreateProduct
{
    public record Command(string Name, string Description, decimal Price, int Stock) : IRequest<Response>;

    public record Response(Guid Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt);

    internal sealed class Handler(AppDbContext context) : IRequestHandler<Command, Response>
    {
        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CreatedAt = DateTime.UtcNow
            };

            context.Products.Add(product);
            await context.SaveChangesAsync(cancellationToken);

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
        app.MapPost("/api/products", async (Command command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/products/{result.Id}", result);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces<Response>(StatusCodes.Status201Created);
    }
}
