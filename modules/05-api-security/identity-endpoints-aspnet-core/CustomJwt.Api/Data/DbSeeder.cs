using Microsoft.AspNetCore.Identity;

namespace CustomJwt.Api.Data;

// Seeds two users so you can log in immediately and see roles land inside the JWT.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await CreateUserAsync(userManager, "admin@codewithmukesh.com", "Admin123!", "Admin");
        await CreateUserAsync(userManager, "user@codewithmukesh.com", "User123!", role: null);
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager, string email, string password, string? role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, password);

        if (role is not null)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
