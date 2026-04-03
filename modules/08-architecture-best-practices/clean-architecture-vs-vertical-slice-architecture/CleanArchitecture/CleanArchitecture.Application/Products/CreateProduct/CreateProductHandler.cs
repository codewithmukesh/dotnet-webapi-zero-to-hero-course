using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using Mediator;

namespace CleanArchitecture.Application.Products.CreateProduct;

public sealed class CreateProductHandler(IProductRepository repository) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async ValueTask<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        await repository.CreateAsync(product, cancellationToken);

        return new CreateProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt);
    }
}
