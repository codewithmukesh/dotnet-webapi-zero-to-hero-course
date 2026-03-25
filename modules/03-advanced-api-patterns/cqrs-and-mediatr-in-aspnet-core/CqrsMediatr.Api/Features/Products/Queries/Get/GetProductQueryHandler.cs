using CqrsMediatr.Api.Features.Products.DTOs;
using CqrsMediatr.Api.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatr.Api.Features.Products.Queries.Get;

public class GetProductQueryHandler(AppDbContext context) : IRequestHandler<GetProductQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return product is null
            ? null
            : new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
