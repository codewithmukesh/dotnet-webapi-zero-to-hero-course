using MediatR;

namespace CQRSMediatR.Features.Products.Commands.Delete;

public record DeleteProductCommand(Guid Id) : IRequest;
