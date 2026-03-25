using CqrsMediatr.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatr.Api.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            new Product("Laptop", "High-performance developer laptop", 1299.99m) { Id = Guid.Parse("d1f3e3b4-1c6a-4e1d-9c3a-2b5f7a8e4d6c") },
            new Product("Mechanical Keyboard", "Cherry MX Brown switches", 149.99m) { Id = Guid.Parse("a2b4c6d8-3e5f-4a7b-9c1d-2e4f6a8b0c3d") },
            new Product("Monitor", "27-inch 4K IPS display", 499.99m) { Id = Guid.Parse("b3c5d7e9-4f6a-5b8c-0d2e-3f5a7b9c1d4e") }
        );
    }
}
