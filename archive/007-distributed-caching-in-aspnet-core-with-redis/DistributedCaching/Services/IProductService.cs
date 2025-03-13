using DistributedCaching.Models;

namespace DistributedCaching.Services;

public interface IProductService
{
    Task<Product> Get(Guid id);
    Task<List<Product>> GetAll();
    Task Add(ProductCreationDto product);
}
