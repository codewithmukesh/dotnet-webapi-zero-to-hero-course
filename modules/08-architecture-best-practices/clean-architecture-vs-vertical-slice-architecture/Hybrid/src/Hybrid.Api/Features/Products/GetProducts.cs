using Hybrid.Api.Shared;
using Hybrid.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hybrid.Api.Features.Products;

public static class GetProducts
{
    public sealed record Response(Guid Id, string Name, decimal Price, int Stock);

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet("/api/products", Handle)
                .WithName(nameof(GetProducts))
                .WithTags("Products");
    }

    internal static async Task<PagedResponse<Response>> Handle(
        AppDbContext db,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        return await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new Response(p.Id, p.Name, p.Price, p.Stock))
            .ToPagedResponseAsync(page, pageSize, cancellationToken);
    }
}
