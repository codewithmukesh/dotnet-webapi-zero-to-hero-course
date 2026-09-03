using Microsoft.EntityFrameworkCore;

namespace EfCoreContainsLargeList.Shared;

public class AppDbContext : DbContext
{
    private readonly ParameterTranslationMode? _mode;
    private readonly SqlCaptureInterceptor? _capture;

    public AppDbContext(
        ParameterTranslationMode? mode = null,
        SqlCaptureInterceptor? capture = null)
    {
        _mode = mode;
        _capture = capture;
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(DbSetup.ConnectionString, sql =>
        {
            if (_mode is { } mode)
            {
                sql.UseParameterizedCollectionMode(mode);
            }
        });

        if (_capture is not null)
        {
            optionsBuilder.AddInterceptors(_capture);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(p => p.Id);
            product.Property(p => p.Id).ValueGeneratedNever();
            product.Property(p => p.Sku).HasMaxLength(32).IsRequired();
            product.Property(p => p.Name).HasMaxLength(128).IsRequired();
            product.Property(p => p.Price).HasPrecision(18, 2);
            product.HasIndex(p => p.CategoryId);
        });
    }
}
