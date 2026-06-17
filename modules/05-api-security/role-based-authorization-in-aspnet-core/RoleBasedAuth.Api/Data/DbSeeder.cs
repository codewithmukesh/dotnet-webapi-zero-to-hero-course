using Microsoft.AspNetCore.Identity;
using RoleBasedAuth.Api.Entities;

namespace RoleBasedAuth.Api.Data;

// Creates the roles and three test users when the app starts,
// so you can log in as each one and see how the role checks behave.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create the roles if they don't exist yet.
        foreach (var role in new[] { Roles.Admin, Roles.Manager, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin who is ALSO a manager - useful for testing endpoints
        // that require both roles at once (AND semantics).
        await CreateUserAsync(userManager,
            email: "admin@codewithmukesh.com",
            password: "Admin123!",
            firstName: "Default",
            lastName: "Admin",
            roles: [Roles.Admin, Roles.Manager]);

        // Manager only.
        await CreateUserAsync(userManager,
            email: "manager@codewithmukesh.com",
            password: "Manager123!",
            firstName: "Default",
            lastName: "Manager",
            roles: [Roles.Manager]);

        // Regular user with no elevated roles.
        await CreateUserAsync(userManager,
            email: "user@codewithmukesh.com",
            password: "User123!",
            firstName: "Default",
            lastName: "User",
            roles: [Roles.User]);
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string[] roles)
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
