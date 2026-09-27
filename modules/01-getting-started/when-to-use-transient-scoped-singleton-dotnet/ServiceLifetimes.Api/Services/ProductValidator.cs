using ServiceLifetimes.Api.Data;

namespace ServiceLifetimes.Api.Services;

// Registered as TRANSIENT: small, stateless, cheap to create.
public sealed class ProductValidator
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public bool IsValid(Product product) =>
        !string.IsNullOrWhiteSpace(product.Name) && product.Price > 0;
}
