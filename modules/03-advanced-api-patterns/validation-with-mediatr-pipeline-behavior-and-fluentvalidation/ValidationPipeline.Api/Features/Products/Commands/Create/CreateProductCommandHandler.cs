using MediatR;
using ValidationPipeline.Api.Domain;
using ValidationPipeline.Api.Features.Products.DTOs;
using ValidationPipeline.Api.Persistence;

namespace ValidationPipeline.Api.Features.Products.Commands.Create;

// Notice there is NO validation code here. By the time a command reaches its
// handler, the pipeline has already validated it.
public class CreateProductCommandHandler(AppDbContext context)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Description, request.Price);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
