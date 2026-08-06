using Microsoft.AspNetCore.Authorization;

namespace PermissionBasedAuth.Api.Authorization;

// One requirement type covers every permission in the system. The permission
// being demanded travels as data on the instance, not as a new class per rule.
public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
