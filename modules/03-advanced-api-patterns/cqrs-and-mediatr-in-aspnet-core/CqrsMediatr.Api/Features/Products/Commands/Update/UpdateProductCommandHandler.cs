using CqrsMediatr.Api.Persistence;
using MediatR;

namespace CqrsMediatr.Api.Features.Products.Commands.Update;

public class UpdateProductCommandHandler(AppDbContext context) : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([request.Id], cancellationToken);

        if (product is null) return false;

        product.Update(request.Name, request.Description, request.Price);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
