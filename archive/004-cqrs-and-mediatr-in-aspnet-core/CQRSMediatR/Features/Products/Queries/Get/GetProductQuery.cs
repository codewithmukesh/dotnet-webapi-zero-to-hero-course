using CQRSMediatR.Features.Products.DTOs;
using MediatR;

namespace CQRSMediatR.Features.Products.Queries.Get;

public record GetProductQuery(Guid Id) : IRequest<ProductDto>;