using EfCoreInterceptors.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreInterceptors.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The interceptor writes the IsDeleted flag; this filter hides the row.
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Product>().HasIndex(p => p.IsDeleted);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
}
