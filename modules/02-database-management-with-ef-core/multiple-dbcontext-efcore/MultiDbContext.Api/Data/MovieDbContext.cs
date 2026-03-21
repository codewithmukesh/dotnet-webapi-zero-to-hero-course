using Microsoft.EntityFrameworkCore;
using MultiDbContext.Api.Entities;

namespace MultiDbContext.Api.Data;

public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).HasMaxLength(300).IsRequired();
            entity.Property(m => m.Genre).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Rating).HasColumnType("decimal(3,1)");
        });
    }
}
