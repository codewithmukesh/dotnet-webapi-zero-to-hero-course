using Microsoft.EntityFrameworkCore;

namespace ResultPattern.Api.Orders;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            order.HasKey(o => o.Id);
            order.Property(o => o.CustomerEmail).HasMaxLength(256).IsRequired();
            order.Property(o => o.Total).HasPrecision(18, 2);
            order.Property(o => o.Status).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<Refund>(refund =>
        {
            refund.HasKey(r => r.Id);
            refund.Property(r => r.Amount).HasPrecision(18, 2);
        });
    }
}
