using MediatR;
using ValidationPipeline.Api.Features.Products.DTOs;

namespace ValidationPipeline.Api.Features.Products.Queries.List;

public record ListProductsQuery : IRequest<List<ProductDto>>;
