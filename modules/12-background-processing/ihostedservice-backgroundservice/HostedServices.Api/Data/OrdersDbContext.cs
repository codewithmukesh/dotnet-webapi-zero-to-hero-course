using Microsoft.EntityFrameworkCore;

namespace HostedServices.Api.Data;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}

public sealed class Order
{
    public int Id { get; set; }
    public required string Customer { get; set; }
    public decimal Total { get; set; }
    public bool Processed { get; set; }
}
