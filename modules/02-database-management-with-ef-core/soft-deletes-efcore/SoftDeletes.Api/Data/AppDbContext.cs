using Microsoft.EntityFrameworkCore;
using SoftDeletes.Api.Entities;

namespace SoftDeletes.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Named query filter — can be selectively disabled
        modelBuilder.Entity<Product>()
            .HasQueryFilter("SoftDelete", p => !p.IsDeleted);

        modelBuilder.Entity<Category>()
            .HasQueryFilter("SoftDelete", c => !c.IsDeleted);

        // Index on IsDeleted for query performance
        modelBuilder.Entity<Product>().HasIndex(p => p.IsDeleted);
        modelBuilder.Entity<Category>().HasIndex(c => c.IsDeleted);

        // Relationship configuration
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
}
