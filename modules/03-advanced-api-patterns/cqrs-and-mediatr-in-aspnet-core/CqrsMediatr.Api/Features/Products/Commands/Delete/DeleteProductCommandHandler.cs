using CqrsMediatr.Api.Persistence;
using MediatR;

namespace CqrsMediatr.Api.Features.Products.Commands.Delete;

public class DeleteProductCommandHandler(AppDbContext context) : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([request.Id], cancellationToken);

        if (product is null) return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
