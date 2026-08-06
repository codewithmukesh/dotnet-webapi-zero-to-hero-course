using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Authorization;

// The source of truth for "which permissions does this role have".
//
// Permissions live as claims on the ROLE (AspNetRoleClaims), not on the user.
// That is what makes them manageable at runtime: grant a permission to Manager
// once and every manager has it on their next request.
//
// Reads go through a cache because the handler hits this on every single
// authorized request. Grants and revokes write through and evict.
public class PermissionStore(IServiceScopeFactory scopeFactory)
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _cache = new(StringComparer.OrdinalIgnoreCase);

    // Resolves the union of permissions across all of the caller's roles.
    public async Task<HashSet<string>> GetForRolesAsync(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            permissions.UnionWith(await GetForRoleAsync(role));
        }

        return permissions;
    }

    public async Task<HashSet<string>> GetForRoleAsync(string role)
    {
        if (_cache.TryGetValue(role, out var cached))
        {
            return cached;
        }

        var permissions = await LoadFromIdentityAsync(role);

        // In a real app this is where HybridCache with a short TTL goes, so the
        // lookup survives a restart and shares across instances.
        _cache[role] = permissions;
        return permissions;
    }

    public async Task<bool> GrantAsync(string role, string permission)
    {
        using var scope = scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var identityRole = await roleManager.FindByNameAsync(role);
        if (identityRole is null)
        {
            return false;
        }

        var existing = await roleManager.GetClaimsAsync(identityRole);
        if (existing.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
        {
            return true;
        }

        await roleManager.AddClaimAsync(identityRole, new Claim(Permissions.ClaimType, permission));

        // Evict so the next request reads the new grant.
        _cache.TryRemove(role, out _);
        return true;
    }

    public async Task<bool> RevokeAsync(string role, string permission)
    {
        using var scope = scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var identityRole = await roleManager.FindByNameAsync(role);
        if (identityRole is null)
        {
            return false;
        }

        var claim = (await roleManager.GetClaimsAsync(identityRole))
            .FirstOrDefault(c => c.Type == Permissions.ClaimType && c.Value == permission);

        if (claim is not null)
        {
            await roleManager.RemoveClaimAsync(identityRole, claim);
        }

        _cache.TryRemove(role, out _);
        return true;
    }

    private async Task<HashSet<string>> LoadFromIdentityAsync(string role)
    {
        using var scope = scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var identityRole = await roleManager.FindByNameAsync(role);
        if (identityRole is null)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var claims = await roleManager.GetClaimsAsync(identityRole);

        return claims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
