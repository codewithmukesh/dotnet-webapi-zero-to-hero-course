using DistributedCaching.Api.Data;
using DistributedCaching.Api.Models;
using DistributedCaching.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        AbortOnConnectFail = false,
        EndPoints = { options.Configuration! }
    };
});

builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var products = app.MapGroup("/products").WithTags("Products");

products.MapGet("/", async (IProductService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetAllAsync(cancellationToken);
    return TypedResults.Ok(result);
})
.WithName("GetAllProducts")
.WithSummary("Get all products")
.WithDescription("Returns all products from the database or Redis cache.");

products.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetByIdAsync(id, cancellationToken);
    return product is not null
        ? TypedResults.Ok(product)
        : Results.NotFound();
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Returns a single product by its ID from the database or Redis cache.");

products.MapPost("/", async (ProductCreationDto request, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.CreateAsync(request, cancellationToken);
    return TypedResults.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithSummary("Create a new product")
.WithDescription("Creates a new product and invalidates the product list cache in Redis.");

app.Run();
