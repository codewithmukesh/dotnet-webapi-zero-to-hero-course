using Mediator;

namespace CleanArchitecture.Application.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock) : IRequest<CreateProductResponse>;

public record CreateProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt);
