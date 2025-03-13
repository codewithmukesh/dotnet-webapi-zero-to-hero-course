using DockerEssentials.Models;

namespace DockerEssentials.Services;

public interface IProductService
{
    Task<Product?> Get(Guid id);
    Task<List<Product>> GetAll();
    Task Add(ProductCreationDto product);
}
