using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationPipeline.Api.Features.Products.DTOs;
using ValidationPipeline.Api.Persistence;

namespace ValidationPipeline.Api.Features.Products.Queries.List;

public class ListProductsQueryHandler(AppDbContext context)
    : IRequestHandler<ListProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        return await context.Products
            .AsNoTracking()
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price))
            .ToListAsync(cancellationToken);
    }
}
