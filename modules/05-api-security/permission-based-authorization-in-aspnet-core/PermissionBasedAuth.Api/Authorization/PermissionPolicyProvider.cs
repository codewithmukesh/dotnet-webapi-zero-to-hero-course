using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Authorization;

// Without this, every permission would need its own AddPolicy call at startup.
// With it, ANY policy name starting with "Permissions." gets a policy built on
// demand - so adding a permission is a data change, not a code change.
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    // ASP.NET Core only ever uses ONE IAuthorizationPolicyProvider. Anything this
    // one does not recognise has to be handed to the default provider, or the
    // named policies registered elsewhere in the app stop resolving.
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Permissions.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                // Not about status codes - 401 vs 403 is decided by whether
                // authentication succeeded. This is here so the policy cannot be
                // satisfied by an anonymous caller if a future handler for this
                // requirement stops inspecting the user.
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackProvider.GetDefaultPolicyAsync();

    // This must return the FALLBACK policy. Returning GetDefaultPolicyAsync()
    // here - a very common copy-paste - silently applies an authenticated-user
    // requirement to every endpoint that never asked for one.
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackProvider.GetFallbackPolicyAsync();
}
