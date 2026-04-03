using Mediator;

namespace CleanArchitecture.Application.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<GetProductByIdResponse?>;

public record GetProductByIdResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt);
