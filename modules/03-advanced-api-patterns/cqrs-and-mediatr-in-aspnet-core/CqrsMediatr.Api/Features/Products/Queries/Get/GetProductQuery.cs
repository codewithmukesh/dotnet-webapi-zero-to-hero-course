using CqrsMediatr.Api.Features.Products.DTOs;
using MediatR;

namespace CqrsMediatr.Api.Features.Products.Queries.Get;

public record GetProductQuery(Guid Id) : IRequest<ProductDto?>;
