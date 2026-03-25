using HybridCaching.Api.Models;

namespace HybridCaching.Api.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(ProductCreationDto request, CancellationToken cancellationToken = default);
}
