using Api.Orders;
using Api.Products;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.Property(p => p.Name).HasMaxLength(200);
            product.Property(p => p.Sku).HasMaxLength(8);
            product.Property(p => p.Price).HasPrecision(18, 2);
            product.HasIndex(p => p.Sku).IsUnique();
        });

        modelBuilder.Entity<Order>(order =>
        {
            order.Property(o => o.Total).HasPrecision(18, 2);
            order.Property(o => o.Status).HasMaxLength(20);
        });
    }
}
