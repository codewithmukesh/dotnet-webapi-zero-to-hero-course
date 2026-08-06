using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Permissions are granted to ROLES, as claims on the role itself.
        // Admin gets everything. Manager and User get progressively less, so the
        // same endpoint returns 200 for one caller and 403 for another.
        await GrantAsync(roleManager, Roles.Admin, Permissions.All);
        await GrantAsync(roleManager, Roles.Manager,
        [
            Permissions.Products.View,
            Permissions.Products.Create
        ]);
        await GrantAsync(roleManager, Roles.User, [Permissions.Products.View]);

        await CreateUserAsync(userManager,
            email: "admin@codewithmukesh.com",
            password: "Admin123!",
            firstName: "Default",
            lastName: "Admin",
            roles: [Roles.Admin]);

        await CreateUserAsync(userManager,
            email: "manager@codewithmukesh.com",
            password: "Manager123!",
            firstName: "Default",
            lastName: "Manager",
            roles: [Roles.Manager]);

        await CreateUserAsync(userManager,
            email: "user@codewithmukesh.com",
            password: "User123!",
            firstName: "Default",
            lastName: "User",
            roles: [Roles.User]);
    }

    private static async Task GrantAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName,
        IEnumerable<string> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return;
        }

        var existing = await roleManager.GetClaimsAsync(role);

        foreach (var permission in permissions)
        {
            if (existing.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
            {
                continue;
            }

            await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
        }
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string firstName, string lastName, string[] roles)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRolesAsync(user, roles);
    }
}
