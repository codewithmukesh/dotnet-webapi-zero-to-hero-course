using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PolicyBasedAuth.Api.Entities;
using PolicyBasedAuth.Api.Services;

namespace PolicyBasedAuth.Api.Data;

// The same three users as the rest of this series, with claims chosen so
// every policy in this sample has a user that passes and a user that fails.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var subscriptions = scope.ServiceProvider.GetRequiredService<SubscriptionStore>();

        foreach (var role in new[] { Roles.Admin, Roles.Manager, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin: STANDARD tier (pro access comes from the Admin role, not subscription).
        // Long tenure - joined over two years ago.
        await CreateUserAsync(userManager, subscriptions,
            email: "admin@codewithmukesh.com",
            password: "Admin123!",
            firstName: "Default",
            lastName: "Admin",
            roles: [Roles.Admin, Roles.Manager],
            department: "engineering",
            joined: "2024-01-15",
            subscriptionTier: "standard");

        // Manager: PREMIUM tier (pro access via subscription), but joined recently -
        // fails the 6-month tenure check.
        await CreateUserAsync(userManager, subscriptions,
            email: "manager@codewithmukesh.com",
            password: "Manager123!",
            firstName: "Default",
            lastName: "Manager",
            roles: [Roles.Manager],
            department: "operations",
            joined: "2026-04-20",
            subscriptionTier: "premium");

        // Regular user: free tier, no elevated roles - but a long-time member.
        await CreateUserAsync(userManager, subscriptions,
            email: "user@codewithmukesh.com",
            password: "User123!",
            firstName: "Default",
            lastName: "User",
            roles: [Roles.User],
            department: null,
            joined: "2024-08-01",
            subscriptionTier: "free");
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        SubscriptionStore subscriptions,
        string email,
        string password,
        string firstName,
        string lastName,
        string[] roles,
        string? department,
        string joined,
        string subscriptionTier)
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

        var claims = new List<Claim> { new(AppClaimTypes.Joined, joined) };
        if (department is not null)
        {
            claims.Add(new Claim(AppClaimTypes.Department, department));
        }

        await userManager.AddClaimsAsync(user, claims);

        // Subscription tier lives OUTSIDE Identity - claims transformation reads it per request.
        subscriptions.SetTier(user.Id, subscriptionTier);
    }
}
