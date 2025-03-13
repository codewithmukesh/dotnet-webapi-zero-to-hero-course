using MediatRPipelineFluentValidation.Features.Products.DTOs;
using MediatR;

namespace MediatRPipelineFluentValidation.Features.Products.Queries.List;

public record ListProductsQuery : IRequest<List<ProductDto>>;