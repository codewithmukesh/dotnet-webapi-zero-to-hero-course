using System.Security.Claims;
using ClaimsBasedAuth.Api.Entities;
using ClaimsBasedAuth.Api.Services;
using Microsoft.AspNetCore.Identity;

namespace ClaimsBasedAuth.Api.Data;

// Seeds the same three users as the role-based article, now with
// department claims in Identity and subscription tiers in the external store.
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

        // Admin: engineering department, premium subscription.
        await CreateUserAsync(userManager, subscriptions,
            email: "admin@codewithmukesh.com",
            password: "Admin123!",
            firstName: "Default",
            lastName: "Admin",
            roles: [Roles.Admin, Roles.Manager],
            department: "engineering",
            subscriptionTier: "premium");

        // Manager: operations department, standard subscription.
        await CreateUserAsync(userManager, subscriptions,
            email: "manager@codewithmukesh.com",
            password: "Manager123!",
            firstName: "Default",
            lastName: "Manager",
            roles: [Roles.Manager],
            department: "operations",
            subscriptionTier: "standard");

        // Regular user: no department claim at all, free tier.
        await CreateUserAsync(userManager, subscriptions,
            email: "user@codewithmukesh.com",
            password: "User123!",
            firstName: "Default",
            lastName: "User",
            roles: [Roles.User],
            department: null,
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

        // Department is a durable fact about the user - store it as an Identity claim.
        // It lands in the AspNetUserClaims table and goes into the token at login.
        if (department is not null)
        {
            await userManager.AddClaimAsync(user, new Claim(AppClaimTypes.Department, department));
        }

        // Subscription tier lives OUTSIDE Identity - claims transformation reads it per request.
        subscriptions.SetTier(user.Id, subscriptionTier);
    }
}
