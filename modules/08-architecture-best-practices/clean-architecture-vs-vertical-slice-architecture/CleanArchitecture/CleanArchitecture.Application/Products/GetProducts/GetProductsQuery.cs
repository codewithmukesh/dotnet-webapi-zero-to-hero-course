using Mediator;

namespace CleanArchitecture.Application.Products.GetProducts;

public record GetProductsQuery : IRequest<List<GetProductsResponse>>;

public record GetProductsResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt);
