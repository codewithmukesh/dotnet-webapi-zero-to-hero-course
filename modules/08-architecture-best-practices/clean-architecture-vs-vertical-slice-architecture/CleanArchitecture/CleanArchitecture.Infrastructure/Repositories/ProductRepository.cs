using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories;

public sealed class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Products.FindAsync([id], cancellationToken);
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
