using CustomUserManagement.Api.Contracts;
using CustomUserManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomUserManagement.Api.Endpoints;

// The admin surface - the part no MVC-framed tutorial gives you as a clean REST API.
// Every route here is admin-only. This whole group is what "user management" really means.
public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // GET /api/admin/users?page=1&pageSize=10&search=jane
        // Paged, searchable list of users, each with their roles. The exact endpoint people
        // keep rebuilding from forum snippets.
        group.MapGet("/", async (
            UserManager<ApplicationUser> userManager,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 10,
            string? search = null) =>
        {
            page = page < 1 ? 1 : page;
            // Cap pageSize at 100 so a client can never ask for the whole table in one call.
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var query = userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.Email!.Contains(term) ||
                    u.FirstName.Contains(term) ||
                    u.LastName.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = new List<UserResponse>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                items.Add(user.ToResponse(roles));
            }

            return Results.Ok(new PagedResult<UserResponse>(items, page, pageSize, totalCount));
        });

        // GET /api/admin/users/{id}
        group.MapGet("/{id}", async (string id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            var roles = await userManager.GetRolesAsync(user);
            return Results.Ok(user.ToResponse(roles));
        });

        // PUT /api/admin/users/{id} - update the profile fields an admin is allowed to change.
        group.MapPut("/{id}", async (
            string id, UpdateUserRequest request, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            return Results.Ok(user.ToResponse(roles));
        });

        // DELETE /api/admin/users/{id}
        group.MapDelete("/{id}", async (string id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            await userManager.DeleteAsync(user);
            return Results.NoContent();
        });

        // POST /api/admin/users/{id}/roles  { "role": "Admin" }  - assign a role.
        group.MapPost("/{id}/roles", async (
            string id, AssignRoleRequest request,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            if (!await roleManager.RoleExistsAsync(request.Role))
            {
                return Results.BadRequest($"Role '{request.Role}' does not exist.");
            }

            await userManager.AddToRoleAsync(user, request.Role);
            return Results.NoContent();
        });

        // DELETE /api/admin/users/{id}/roles/{role} - remove a role.
        group.MapDelete("/{id}/roles/{role}", async (
            string id, string role, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            await userManager.RemoveFromRoleAsync(user, role);
            return Results.NoContent();
        });

        // POST /api/admin/users/{id}/lock - temporary security lockout (Identity's own feature).
        group.MapPost("/{id}/lock", async (string id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            return Results.NoContent();
        });

        // POST /api/admin/users/{id}/unlock - clear the lockout.
        group.MapPost("/{id}/unlock", async (string id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
            return Results.NoContent();
        });

        // POST /api/admin/users/{id}/disable - permanent admin switch (our IsActive flag).
        // Different from lockout - see the article's "Lock vs Disable" section.
        group.MapPost("/{id}/disable", async (string id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            user.IsActive = false;
            await userManager.UpdateAsync(user);
            return Results.NoContent();
        });

        // POST /api/admin/users/{id}/enable - turn the account back on.
        group.MapPost("/{id}/enable", async (string id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            user.IsActive = true;
            await userManager.UpdateAsync(user);
            return Results.NoContent();
        });
    }
}
