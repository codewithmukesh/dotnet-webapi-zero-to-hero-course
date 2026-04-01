using Mediator;

namespace CleanArchitecture.Application.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock) : IRequest<UpdateProductResponse?>;

public record UpdateProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt);
