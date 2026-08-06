using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PermissionBasedAuth.Api.Auth;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Authorization;

// Decides whether the caller holds the permission the endpoint asked for.
public class PermissionAuthorizationHandler(
    PermissionStore store,
    IOptions<PermissionSettings> settings)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // No identity means no permissions. Leaving the requirement unmet here
        // produces a 401 rather than a 403, which is what an anonymous caller
        // should see.
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return;
        }

        var granted = settings.Value.Source == PermissionSource.Token
            ? ReadFromToken(context.User)
            : await store.GetForRolesAsync(ReadRoles(context.User));

        if (granted.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        // No context.Fail() - another handler for the same requirement may still
        // succeed. Fail() would veto them all.
    }

    private static HashSet<string> ReadFromToken(ClaimsPrincipal user) =>
        user.FindAll(Permissions.ClaimType)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    // Read roles through the identity's configured RoleClaimType. Hard-coding
    // ClaimTypes.Role breaks the moment MapInboundClaims is false and the token
    // carries a short "role" claim instead of the long WS-Fed URI.
    private static HashSet<string> ReadRoles(ClaimsPrincipal user) =>
        user.Identities
            .SelectMany(identity => identity.FindAll(identity.RoleClaimType))
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
