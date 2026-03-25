using CqrsMediatr.Api.Features.Products.DTOs;
using MediatR;

namespace CqrsMediatr.Api.Features.Products.Queries.List;

public record ListProductsQuery : IRequest<List<ProductDto>>;
