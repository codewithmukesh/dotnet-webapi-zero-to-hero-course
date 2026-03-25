using DistributedCaching.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedCaching.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(1000).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });
    }
}
