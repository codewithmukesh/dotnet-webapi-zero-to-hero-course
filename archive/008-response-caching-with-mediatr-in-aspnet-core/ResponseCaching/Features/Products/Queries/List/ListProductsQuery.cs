using MediatR;
using ResponseCaching.Features.Products.Dtos;

namespace ResponseCaching.Features.Products.Queries.List;

public record ListProductsQuery : IRequest<List<ProductDto>>;