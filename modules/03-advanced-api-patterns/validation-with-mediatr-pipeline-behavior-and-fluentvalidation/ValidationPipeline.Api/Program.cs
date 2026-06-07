using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ValidationPipeline.Api.Behaviors;
using ValidationPipeline.Api.Exceptions;
using ValidationPipeline.Api.Features.Products.Commands.Create;
using ValidationPipeline.Api.Features.Products.Queries.List;
using ValidationPipeline.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ValidationPipelineDb"));

// Register MediatR and plug the two behaviors into its pipeline.
// Order matters: logging runs first, then validation.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(RequestResponseLoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Discover and register every FluentValidation validator in this project.
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Turn the validation exception into a clean ProblemDetails response.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/products", async (ISender mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new ListProductsQuery(), ct)));

app.MapPost("/products", async (CreateProductCommand command, ISender mediator, CancellationToken ct) =>
{
    var product = await mediator.Send(command, ct);
    return Results.Created($"/products/{product.Id}", product);
});

app.Run();
