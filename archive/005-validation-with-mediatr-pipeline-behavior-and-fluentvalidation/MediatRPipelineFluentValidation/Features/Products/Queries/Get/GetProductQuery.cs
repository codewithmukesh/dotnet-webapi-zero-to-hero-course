using MediatRPipelineFluentValidation.Features.Products.DTOs;
using MediatR;

namespace MediatRPipelineFluentValidation.Features.Products.Queries.Get;

public record GetProductQuery(Guid Id) : IRequest<ProductDto>;