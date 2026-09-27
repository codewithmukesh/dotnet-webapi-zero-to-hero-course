using Microsoft.EntityFrameworkCore;

namespace ServiceLifetimes.Api.Data;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Mechanical Keyboard", Price = 129.99m },
            new Product { Id = 2, Name = "USB-C Hub", Price = 49.50m },
            new Product { Id = 3, Name = "27-inch Monitor", Price = 329.00m });
    }
}
