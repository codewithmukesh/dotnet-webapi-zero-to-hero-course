using MediatR;

namespace ResponseCaching.Features.Products.Commands.Create;

public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<Guid>;