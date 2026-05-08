using ApiKeyAuth.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiKeyAuth.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(k => k.Id);
            entity.Property(k => k.Prefix).IsRequired().HasMaxLength(20);
            entity.Property(k => k.KeyHash).IsRequired().HasMaxLength(64);
            entity.Property(k => k.Name).IsRequired().HasMaxLength(100);
            entity.Property(k => k.OwnerId).IsRequired().HasMaxLength(100);
            entity.Property(k => k.Scopes).HasMaxLength(500);

            entity.HasIndex(k => k.KeyHash).IsUnique();
            entity.HasIndex(k => k.Prefix);
        });
    }
}
