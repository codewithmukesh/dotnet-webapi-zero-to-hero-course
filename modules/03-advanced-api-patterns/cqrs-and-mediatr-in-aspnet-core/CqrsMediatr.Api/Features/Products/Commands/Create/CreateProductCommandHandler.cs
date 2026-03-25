using CqrsMediatr.Api.Domain;
using CqrsMediatr.Api.Features.Products.DTOs;
using CqrsMediatr.Api.Persistence;
using MediatR;

namespace CqrsMediatr.Api.Features.Products.Commands.Create;

public class CreateProductCommandHandler(AppDbContext context) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Description, request.Price);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
