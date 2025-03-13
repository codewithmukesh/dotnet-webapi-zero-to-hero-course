using DistributedCaching.Models;
using DistributedCaching.Persistence;
using DistributedCaching.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("dotnetSeries"));
                });
builder.Services.AddTransient<IProductService, ProductService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost";
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
    {
        AbortOnConnectFail = true,
        EndPoints = { options.Configuration }
    };
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

app.MapGet("/products", async (IProductService service) =>
{
    var products = await service.GetAll();
    return Results.Ok(products);
});

app.MapGet("/products/{id:guid}", async (Guid id, IProductService service) =>
{
    var product = await service.Get(id);
    return Results.Ok(product);
});

app.MapPost("/products", async (ProductCreationDto product, IProductService service) =>
{
    await service.Add(product);
    return Results.Created();
});

app.UseHttpsRedirection();
app.Run();