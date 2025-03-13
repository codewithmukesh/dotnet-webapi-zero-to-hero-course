using MediatR;
using ResponseCaching.Caching;
using ResponseCaching.Features.Products.Dtos;

namespace ResponseCaching.Features.Products.Queries.Get;

public record GetProductQuery(Guid Id) : IRequest<ProductDto>, ICacheable
{
    public bool BypassCache => false;

    public string CacheKey => $"product:{Id}";

    public int SlidingExpirationInMinutes => 30;

    public int AbsoluteExpirationInMinutes => 60;
}
