using MediatR;

namespace CqrsMediatr.Api.Features.Products.Commands.Delete;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
