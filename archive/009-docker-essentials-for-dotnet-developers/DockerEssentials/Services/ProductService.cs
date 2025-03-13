using DockerEssentials.Models;
using DockerEssentials.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DockerEssentials.Services;

public class ProductService(AppDbContext context) : IProductService
{
    public async Task Add(ProductCreationDto request)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }

    public async Task<Product?> Get(Guid id)
    {
        return await context.Products.FindAsync(id);
    }

    public async Task<List<Product>> GetAll()
    {
        return await context.Products.ToListAsync();
    }
}
