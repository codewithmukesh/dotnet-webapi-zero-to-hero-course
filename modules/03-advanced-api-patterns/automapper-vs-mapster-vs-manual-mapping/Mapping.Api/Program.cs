using AutoMapper;
using Mapping.Api.Contracts;
using Mapping.Api.Data;
using Mapping.Api.Mapping.AutoMapper;
using Mapping.Api.Mapping.Manual;
using Mapping.Api.Mapping.Mapperly;
using Mapping.Api.Mapping.Mapster;
using Mapster;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<ProductProfile>());

ProductMapsterConfig.Register(TypeAdapterConfig.GlobalSettings);

builder.Services.AddSingleton<ProductMapperlyMapper>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/products/automapper", (IMapper mapper) =>
{
    var product = ProductSeed.Sample();
    return mapper.Map<ProductResponse>(product);
});

app.MapGet("/products/mapster", () =>
{
    var product = ProductSeed.Sample();
    return product.Adapt<ProductResponse>();
});

app.MapGet("/products/mapperly", (ProductMapperlyMapper mapper) =>
{
    var product = ProductSeed.Sample();
    return mapper.ToResponse(product);
});

app.MapGet("/products/manual", () =>
{
    var product = ProductSeed.Sample();
    return product.ToResponse();
});

app.Run();
