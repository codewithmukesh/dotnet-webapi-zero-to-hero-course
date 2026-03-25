using CqrsMediatr.Api.Features.Products.DTOs;
using MediatR;

namespace CqrsMediatr.Api.Features.Products.Commands.Create;

public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<ProductDto>;
