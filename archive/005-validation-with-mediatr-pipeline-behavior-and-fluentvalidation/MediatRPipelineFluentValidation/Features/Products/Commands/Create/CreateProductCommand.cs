using MediatR;

namespace MediatRPipelineFluentValidation.Features.Products.Commands.Create;

public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<Guid>;