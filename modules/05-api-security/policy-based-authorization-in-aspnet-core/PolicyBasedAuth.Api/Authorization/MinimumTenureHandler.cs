using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuth.Api.Entities;

namespace PolicyBasedAuth.Api.Authorization;

// Computes account tenure from the "joined" claim - a comparison RequireClaim
// could never express, because RequireClaim only does exact matching.
public class MinimumTenureHandler(TimeProvider timeProvider) : AuthorizationHandler<MinimumTenureRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumTenureRequirement requirement)
    {
        var joinedClaim = context.User.FindFirst(AppClaimTypes.Joined);

        if (joinedClaim is null || !DateOnly.TryParse(joinedClaim.Value, out var joined))
        {
            // Missing or malformed claim - just don't succeed. No Fail() needed.
            return Task.CompletedTask;
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var tenureMonths = ((today.Year - joined.Year) * 12) + today.Month - joined.Month;

        if (tenureMonths >= requirement.Months)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
