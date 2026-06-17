using System.Security.Claims;
using ClaimsBasedAuth.Api.Entities;
using ClaimsBasedAuth.Api.Services;
using Microsoft.AspNetCore.Authentication;

namespace ClaimsBasedAuth.Api.Auth;

// Runs AFTER token validation on every request. Enriches the ClaimsPrincipal with
// claims that should NOT live inside the token - here, the subscription tier,
// which can change at any moment and must always be current.
public class SubscriptionClaimsTransformation(SubscriptionStore subscriptions) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Never add the same claim twice - transformations can run more than once per request.
        if (principal.HasClaim(c => c.Type == AppClaimTypes.Subscription))
        {
            return Task.FromResult(principal);
        }

        var userId = principal.FindFirstValue("sub");
        if (userId is null)
        {
            return Task.FromResult(principal);
        }

        var tier = subscriptions.GetTier(userId);

        // Clone before modifying - the incoming principal should be treated as read-only.
        var clone = principal.Clone();
        var identity = (ClaimsIdentity)clone.Identity!;
        identity.AddClaim(new Claim(AppClaimTypes.Subscription, tier));

        return Task.FromResult(clone);
    }
}
