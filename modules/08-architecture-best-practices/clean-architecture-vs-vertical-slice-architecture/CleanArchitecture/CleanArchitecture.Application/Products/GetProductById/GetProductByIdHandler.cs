using CleanArchitecture.Application.Common.Interfaces;
using Mediator;

namespace CleanArchitecture.Application.Products.GetProductById;

public sealed class GetProductByIdHandler(IProductRepository repository) : IRequestHandler<GetProductByIdQuery, GetProductByIdResponse?>
{
    public async ValueTask<GetProductByIdResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new GetProductByIdResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt);
    }
}
