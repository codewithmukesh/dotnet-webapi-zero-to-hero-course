using CustomUserManagement.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace CustomUserManagement.Api.Data;

// Seeds the Admin and User roles plus a super-admin you can log in with right away.
// Run this once at startup - the order matters: roles must exist before you assign one.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (await userManager.FindByEmailAsync("admin@codewithmukesh.com") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@codewithmukesh.com",
                Email = "admin@codewithmukesh.com",
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
