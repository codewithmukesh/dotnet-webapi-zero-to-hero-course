using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuth.Api.Entities;

namespace PolicyBasedAuth.Api.Authorization;

// Handler #1 for ProUserRequirement: a premium subscription satisfies it.
public class PremiumSubscriptionHandler : AuthorizationHandler<ProUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProUserRequirement requirement)
    {
        if (context.User.HasClaim(AppClaimTypes.Subscription, "premium"))
        {
            context.Succeed(requirement);
        }

        // Never call Fail() just because THIS handler didn't match -
        // another handler for the same requirement may still succeed.
        return Task.CompletedTask;
    }
}
