using System.Security.Claims;
using RoleBasedAuth.Api.Entities;
using RoleBasedAuth.Api.Models;
using RoleBasedAuth.Api.Services;

namespace RoleBasedAuth.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        // RequireAuthorization() on the group = every endpoint below needs a valid token.
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        // Any authenticated user can view products. No role needed.
        group.MapGet("/", (ProductStore store) => Results.Ok(store.GetAll()));

        // Admin OR Manager can create products.
        // Passing multiple roles to ONE RequireRole call means "any of these".
        group.MapPost("/", (CreateProductRequest request, ProductStore store) =>
            {
                var product = store.Add(request);
                return Results.Created($"/api/products/{product.Id}", product);
            })
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));

        // Only Admin can delete. Manager gets a 403 here.
        group.MapDelete("/{id:int}", (int id, ProductStore store) =>
                store.Delete(id) ? Results.NoContent() : Results.NotFound())
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // Restock uses a NAMED policy registered in Program.cs.
        // Same result as the inline RequireRole above, but reusable across endpoints.
        group.MapPut("/{id:int}/restock", (int id) =>
                Results.Ok($"Product {id} restocked."))
            .RequireAuthorization("ManagerOnly");

        // Chaining two RequireRole calls = two separate requirements = AND.
        // The caller must hold BOTH roles. Only the seeded admin (Admin + Manager) passes.
        group.MapGet("/audit", () =>
                Results.Ok("Stock audit report. You hold both the Admin and Manager roles."))
            .RequireAuthorization(policy => policy
                .RequireRole(Roles.Admin)
                .RequireRole(Roles.Manager));

        // Role checks in code with IsInRole - useful when the response
        // changes by role instead of being allowed/denied outright.
        group.MapGet("/dashboard", (ClaimsPrincipal user) =>
        {
            var greeting = $"Hello {user.Identity?.Name}.";

            if (user.IsInRole(Roles.Admin))
            {
                return Results.Ok($"{greeting} Full dashboard: sales, inventory, and user management.");
            }

            return user.IsInRole(Roles.Manager)
                ? Results.Ok($"{greeting} Manager dashboard: inventory and restock queue.")
                : Results.Ok($"{greeting} Your orders and saved items.");
        });
    }
}
