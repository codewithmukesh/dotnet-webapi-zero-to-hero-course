using CleanArchitecture.Application.Common.Interfaces;
using Mediator;

namespace CleanArchitecture.Application.Products.GetProducts;

public sealed class GetProductsHandler(IProductRepository repository) : IRequestHandler<GetProductsQuery, List<GetProductsResponse>>
{
    public async ValueTask<List<GetProductsResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(cancellationToken);

        return products.Select(p => new GetProductsResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.CreatedAt)).ToList();
    }
}
