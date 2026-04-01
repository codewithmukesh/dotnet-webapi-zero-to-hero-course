using CleanArchitecture.Application.Products.CreateProduct;
using CleanArchitecture.Application.Products.DeleteProduct;
using CleanArchitecture.Application.Products.GetProductById;
using CleanArchitecture.Application.Products.GetProducts;
using CleanArchitecture.Application.Products.UpdateProduct;
using Mediator;

namespace CleanArchitecture.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapPost("/", async (CreateProductCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/products/{result.Id}", result);
        })
        .WithName("CreateProduct")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created);

        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductsQuery(), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetProducts")
        .Produces<List<GetProductsResponse>>();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetProductById")
        .Produces<GetProductByIdResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Stock);
            var result = await sender.Send(command, cancellationToken);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("UpdateProduct")
        .Produces<UpdateProductResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public record UpdateProductRequest(string Name, string Description, decimal Price, int Stock);
