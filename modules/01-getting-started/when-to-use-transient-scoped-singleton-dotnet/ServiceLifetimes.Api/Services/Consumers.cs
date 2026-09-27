using ServiceLifetimes.Api.Caching;
using ServiceLifetimes.Api.Data;

namespace ServiceLifetimes.Api.Services;

public sealed record InstanceIds(string Validator, string DbContext, string Cache);

// Two classes that both depend on all three lifetimes, so one request resolves each service twice.
public sealed class ProductService(ProductValidator validator, AppDbContext db, ProductCache cache)
{
    public InstanceIds Describe() =>
        new(validator.InstanceId.Short(), db.ContextId.InstanceId.Short(), cache.InstanceId.Short());
}

public sealed class PricingService(ProductValidator validator, AppDbContext db, ProductCache cache)
{
    public InstanceIds Describe() =>
        new(validator.InstanceId.Short(), db.ContextId.InstanceId.Short(), cache.InstanceId.Short());
}

public static class GuidExtensions
{
    // The first 4 hex characters are enough to tell instances apart in the demo output.
    public static string Short(this Guid id) => id.ToString("N")[..4];
}
