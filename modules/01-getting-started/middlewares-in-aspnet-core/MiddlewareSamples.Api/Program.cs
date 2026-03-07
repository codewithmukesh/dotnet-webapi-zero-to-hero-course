using MiddlewareSamples.Api.Extensions;
using MiddlewareSamples.Api.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Register IMiddleware implementations with DI
builder.Services.AddTransient<CorrelationIdMiddleware>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Middleware pipeline — order matters!
app.UseMaintenance();
app.UseCorrelationId();
app.UseRequestLogging();

app.MapGet("/", () => "Hello from Middleware Samples API!")
    .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.MapGet("/products", () =>
{
    var products = new[]
    {
        new { Id = 1, Name = "Laptop", Price = 999.99 },
        new { Id = 2, Name = "Mouse", Price = 29.99 },
        new { Id = 3, Name = "Keyboard", Price = 79.99 }
    };
    return Results.Ok(products);
}).WithName("GetProducts");

app.Run();
