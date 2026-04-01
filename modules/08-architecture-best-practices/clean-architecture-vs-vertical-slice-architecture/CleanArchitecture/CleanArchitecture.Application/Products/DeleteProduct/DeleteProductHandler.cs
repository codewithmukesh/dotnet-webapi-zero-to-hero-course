using CleanArchitecture.Application.Common.Interfaces;
using Mediator;

namespace CleanArchitecture.Application.Products.DeleteProduct;

public sealed class DeleteProductHandler(IProductRepository repository) : IRequestHandler<DeleteProductCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        await repository.DeleteAsync(product, cancellationToken);

        return true;
    }
}
