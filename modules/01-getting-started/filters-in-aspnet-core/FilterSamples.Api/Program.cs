using FilterSamples.Api.EndpointFilters;
using FilterSamples.Api.Extensions;
using FilterSamples.Api.Filters;
using FilterSamples.Api.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<TimingResourceFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ResponseWrappingResultFilter>();
});

builder.Services.AddOpenApi();
builder.Services.AddFilterServices();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

// Minimal API endpoints with endpoint filters
var products = app.MapGroup("/api/minimal/products");

products.MapGet("/", () => Results.Ok(new[]
{
    new Product(1, "Keyboard", 79.99m),
    new Product(2, "Mouse", 49.99m)
}));

products.MapPost("/", (CreateProductRequest request) =>
    Results.Created($"/api/minimal/products/1", new Product(1, request.Name, request.Price, request.Description)))
    .AddEndpointFilter<ValidationEndpointFilter<CreateProductRequest>>();

app.Run();
