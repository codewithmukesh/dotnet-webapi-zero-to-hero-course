using HostedServices.Api.Data;
using HostedServices.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core 10 in-memory DB so the OrderProcessor BackgroundService has something
// to read against. Real apps would use SQL Server, PostgreSQL, etc.
builder.Services.AddDbContext<OrdersDbContext>(opts =>
    opts.UseInMemoryDatabase("orders-demo"));

// Gotcha 1 fix: in .NET 6+ an unhandled exception in ExecuteAsync crashes the host.
// Switch to Ignore so transient background errors do not take the API down.
builder.Services.Configure<HostOptions>(opts =>
{
    opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Hosted services run in registration order on startup,
// reverse registration order on shutdown.
builder.Services.AddHostedService<StartupTask>();                    // one-shot, raw IHostedService
builder.Services.AddHostedService<HeartbeatHostedService>();          // raw IHostedService loop (illustrative)
builder.Services.AddHostedService<HeartbeatBackgroundService>();      // same task, BackgroundService
builder.Services.AddHostedService<OrderProcessor>();                  // scoped service via IServiceScopeFactory
builder.Services.AddHostedService<CpuYieldingService>();              // Task.Yield + chunked work

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Seed a few orders so OrderProcessor has work to do.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    if (!db.Orders.Any())
    {
        db.Orders.AddRange(
            new Order { Id = 1, Customer = "Acme",   Total = 49.99m,  Processed = false },
            new Order { Id = 2, Customer = "Globex", Total = 129.00m, Processed = false },
            new Order { Id = 3, Customer = "Initech", Total = 12.50m, Processed = false });
        db.SaveChanges();
    }
}

app.MapGet("/orders", async (OrdersDbContext db, CancellationToken ct) =>
    await db.Orders.AsNoTracking().ToListAsync(ct));

app.MapGet("/", () => Results.Ok(new
{
    message = "HostedServices demo API. See logs for BackgroundService output.",
    endpoints = new[] { "/orders", "/scalar/v1" }
}));

await app.RunAsync();
