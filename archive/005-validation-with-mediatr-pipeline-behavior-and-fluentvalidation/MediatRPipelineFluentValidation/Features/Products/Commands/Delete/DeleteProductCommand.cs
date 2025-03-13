using MediatR;

namespace MediatRPipelineFluentValidation.Features.Products.Commands.Delete;

public record DeleteProductCommand(Guid Id) : IRequest;
