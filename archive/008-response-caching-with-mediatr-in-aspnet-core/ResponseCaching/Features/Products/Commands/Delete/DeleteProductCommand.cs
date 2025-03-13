using MediatR;

namespace ResponseCaching.Features.Products.Commands.Delete;

public record DeleteProductCommand(Guid Id) : IRequest;
