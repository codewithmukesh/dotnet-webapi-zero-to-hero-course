using PermissionBasedAuth.Api.Entities;
using PermissionBasedAuth.Api.Models;
using PermissionBasedAuth.Api.Services;

namespace PermissionBasedAuth.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // The permission string IS the policy name. No AddPolicy call anywhere -
        // PermissionPolicyProvider builds each one the first time it is asked for.
        group.MapGet("/", (ProductStore store) => Results.Ok(store.GetAll()))
            .RequireAuthorization(Permissions.Products.View);

        group.MapPost("/", (SaveProductRequest request, ProductStore store) =>
            {
                var product = store.Add(request);
                return Results.Created($"/api/products/{product.Id}", product);
            })
            .RequireAuthorization(Permissions.Products.Create);

        group.MapPut("/{id:int}", (int id, SaveProductRequest request, ProductStore store) =>
            {
                var product = store.Update(id, request);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .RequireAuthorization(Permissions.Products.Edit);

        group.MapDelete("/{id:int}", (int id, ProductStore store) =>
                store.Delete(id) ? Results.NoContent() : Results.NotFound())
            .RequireAuthorization(Permissions.Products.Delete);
    }
}
