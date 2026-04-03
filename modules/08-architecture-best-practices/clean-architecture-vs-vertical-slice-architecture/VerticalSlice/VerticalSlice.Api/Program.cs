using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VerticalSlice.Api.Data;
using VerticalSlice.Api.Features.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

CreateProduct.Map(app);
GetProducts.Map(app);
GetProductById.Map(app);
UpdateProduct.Map(app);
DeleteProduct.Map(app);

app.Run();
