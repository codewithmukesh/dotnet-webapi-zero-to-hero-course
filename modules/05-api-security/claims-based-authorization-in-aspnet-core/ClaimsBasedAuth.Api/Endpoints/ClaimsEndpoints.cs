using System.Security.Claims;
using ClaimsBasedAuth.Api.Entities;

namespace ClaimsBasedAuth.Api.Endpoints;

public static class ClaimsEndpoints
{
    public static void MapClaimsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Claims")
            .RequireAuthorization();

        // Read claims from the current user - the foundation of everything else.
        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            // FindFirstValue returns the first claim's value, or null if absent.
            var userId = user.FindFirstValue("sub");
            var email = user.FindFirstValue("email");
            var department = user.FindFirstValue(AppClaimTypes.Department);
            var subscription = user.FindFirstValue(AppClaimTypes.Subscription);

            return Results.Ok(new
            {
                UserId = userId,
                Email = email,
                Department = department ?? "none",
                Subscription = subscription,
                // Every claim the server sees for this request - roles included.
                AllClaims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        });

        // Claim PRESENCE check: the user must have a department claim - any value.
        group.MapGet("/exports", () =>
                Results.Ok("Export started. Any user assigned to a department can do this."))
            .RequireAuthorization(policy => policy.RequireClaim(AppClaimTypes.Department));

        // Claim VALUE check: the department claim must be exactly "engineering".
        group.MapGet("/deployments", () =>
                Results.Ok("Deployment dashboard. Engineering only."))
            .RequireAuthorization(policy =>
                policy.RequireClaim(AppClaimTypes.Department, "engineering"));

        // The subscription claim is NOT in the token - claims transformation added it
        // from the subscription store on this very request.
        group.MapGet("/reports/premium", () =>
                Results.Ok("Premium analytics report. Generated fresh for premium subscribers."))
            .RequireAuthorization(policy =>
                policy.RequireClaim(AppClaimTypes.Subscription, "premium"));

        // HasClaim for in-code branching - same data, checked imperatively.
        group.MapGet("/workspace", (ClaimsPrincipal user) =>
        {
            if (user.HasClaim(AppClaimTypes.Department, "engineering"))
            {
                return Results.Ok("Engineering workspace: build pipelines and deployment logs.");
            }

            return user.HasClaim(c => c.Type == AppClaimTypes.Department)
                ? Results.Ok("Department workspace: shared documents and reports.")
                : Results.Ok("Personal workspace.");
        });
    }
}
