using CleanArchitecture.Application.Common.Interfaces;
using Mediator;

namespace CleanArchitecture.Application.Products.UpdateProduct;

public sealed class UpdateProductHandler(IProductRepository repository) : IRequestHandler<UpdateProductCommand, UpdateProductResponse?>
{
    public async ValueTask<UpdateProductResponse?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        await repository.UpdateAsync(product, cancellationToken);

        return new UpdateProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt);
    }
}
