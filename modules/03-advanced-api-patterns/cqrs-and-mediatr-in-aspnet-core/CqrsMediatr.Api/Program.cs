using System.Reflection;
using CqrsMediatr.Api.Features.Products.Commands.Create;
using CqrsMediatr.Api.Features.Products.Commands.Delete;
using CqrsMediatr.Api.Features.Products.Commands.Update;
using CqrsMediatr.Api.Features.Products.Notifications;
using CqrsMediatr.Api.Features.Products.Queries.Get;
using CqrsMediatr.Api.Features.Products.Queries.List;
using CqrsMediatr.Api.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CqrsMediatrDb"));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/products/{id:guid}", async (Guid id, ISender mediatr) =>
{
    var product = await mediatr.Send(new GetProductQuery(id));
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapGet("/products", async (ISender mediatr) =>
{
    var products = await mediatr.Send(new ListProductsQuery());
    return Results.Ok(products);
});

app.MapPost("/products", async (CreateProductCommand command, IMediator mediatr) =>
{
    var product = await mediatr.Send(command);
    await mediatr.Publish(new ProductCreatedNotification(product.Id, product.Name));
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductCommand command, ISender mediatr) =>
{
    if (id != command.Id) return Results.BadRequest();
    var result = await mediatr.Send(command);
    return result ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/products/{id:guid}", async (Guid id, ISender mediatr) =>
{
    var result = await mediatr.Send(new DeleteProductCommand(id));
    return result ? Results.NoContent() : Results.NotFound();
});

app.Run();
