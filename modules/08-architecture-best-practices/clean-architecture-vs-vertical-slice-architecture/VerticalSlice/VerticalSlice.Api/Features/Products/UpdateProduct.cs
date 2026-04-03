using Mediator;
using VerticalSlice.Api.Data;

namespace VerticalSlice.Api.Features.Products;

public static class UpdateProduct
{
    public record Command(Guid Id, string Name, string Description, decimal Price, int Stock) : IRequest<Response?>;

    public record Request(string Name, string Description, decimal Price, int Stock);

    public record Response(Guid Id, string Name, string Description, decimal Price, int Stock, DateTime CreatedAt);

    internal sealed class Handler(AppDbContext context) : IRequestHandler<Command, Response?>
    {
        public async ValueTask<Response?> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([command.Id], cancellationToken);

            if (product is null)
            {
                return null;
            }

            product.Name = command.Name;
            product.Description = command.Description;
            product.Price = command.Price;
            product.Stock = command.Stock;

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
        app.MapPut("/api/products/{id:guid}", async (Guid id, Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new Command(id, request.Name, request.Description, request.Price, request.Stock);
            var result = await sender.Send(command, cancellationToken);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .Produces<Response>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
