using MediatR;
using ValidationPipeline.Api.Features.Products.DTOs;

namespace ValidationPipeline.Api.Features.Products.Commands.Create;

public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<ProductDto>;
