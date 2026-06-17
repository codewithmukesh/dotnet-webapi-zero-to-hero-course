using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PolicyBasedAuth.Api.Entities;
using PolicyBasedAuth.Api.Services;

namespace PolicyBasedAuth.Api.Auth;

// Carried over from the claims-based authorization article: enriches the
// ClaimsPrincipal with the user's CURRENT subscription tier on every request.
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
