using CQRSMediatR.Features.Products.DTOs;
using MediatR;

namespace CQRSMediatR.Features.Products.Queries.List;

public record ListProductsQuery : IRequest<List<ProductDto>>;