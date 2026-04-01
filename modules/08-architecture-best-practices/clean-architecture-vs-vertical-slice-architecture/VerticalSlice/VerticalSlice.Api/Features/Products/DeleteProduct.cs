using Mediator;
using VerticalSlice.Api.Data;

namespace VerticalSlice.Api.Features.Products;

public static class DeleteProduct
{
    public record Command(Guid Id) : IRequest<bool>;

    internal sealed class Handler(AppDbContext context) : IRequestHandler<Command, bool>
    {
        public async ValueTask<bool> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([command.Id], cancellationToken);

            if (product is null)
            {
                return false;
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new Command(id), cancellationToken);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
