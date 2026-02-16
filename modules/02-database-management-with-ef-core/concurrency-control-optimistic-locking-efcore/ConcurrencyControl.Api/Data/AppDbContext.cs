using ConcurrencyControl.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyControl.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Stock).IsRequired();
            entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
            entity.Property(p => p.CreatedAt).IsRequired();

            // Configure RowVersion as a concurrency token using PostgreSQL's xmin system column
            entity.Property(p => p.RowVersion)
                .IsRowVersion();
        });
    }
}
