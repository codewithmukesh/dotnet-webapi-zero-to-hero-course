using Microsoft.AspNetCore.Mvc;
using PermissionBasedAuth.Api.Authorization;
using PermissionBasedAuth.Api.Entities;
using PermissionBasedAuth.Api.Models;

namespace PermissionBasedAuth.Api.Endpoints;

// This is the payoff of the whole pattern: changing what a role can do is an
// API call, not a redeploy. Role-based authorization cannot do this because the
// role names are compiled into the endpoints.
public static class AdminPermissionEndpoints
{
    public static void MapAdminPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/roles")
            .WithTags("Admin - Permissions")
            .RequireAuthorization(Permissions.Products.Edit);

        group.MapGet("/{role}/permissions", async (string role, PermissionStore store) =>
        {
            var permissions = await store.GetForRoleAsync(role);
            return Results.Ok(new RolePermissionsResponse(role, permissions.Order()));
        });

        group.MapPost("/{role}/permissions", async (
            string role, PermissionRequest request, PermissionStore store) =>
        {
            if (!Permissions.All.Contains(request.Permission))
            {
                return Results.BadRequest($"Unknown permission '{request.Permission}'.");
            }

            return await store.GrantAsync(role, request.Permission)
                ? Results.Ok($"Granted '{request.Permission}' to '{role}'.")
                : Results.NotFound($"Role '{role}' not found.");
        });

        // Minimal APIs will not infer a body on DELETE, so [FromBody] is required here.
        group.MapDelete("/{role}/permissions", async (
            string role, [FromBody] PermissionRequest request, PermissionStore store) =>
        {
            return await store.RevokeAsync(role, request.Permission)
                ? Results.Ok($"Revoked '{request.Permission}' from '{role}'.")
                : Results.NotFound($"Role '{role}' not found.");
        });
    }
}
