using Microsoft.EntityFrameworkCore;
using MultiDbContext.Api.Entities;

namespace MultiDbContext.Api.Data;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<ApiEvent> ApiEvents => Set<ApiEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Endpoint).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Method).HasMaxLength(10).IsRequired();
        });
    }
}
