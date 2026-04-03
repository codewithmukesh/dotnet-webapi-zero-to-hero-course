using Mediator;

namespace CleanArchitecture.Application.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
