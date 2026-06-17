using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuth.Api.Entities;

namespace PolicyBasedAuth.Api.Authorization;

// Handler #2 for the SAME ProUserRequirement: admins get pro access regardless
// of subscription. Two handlers for one requirement = OR semantics.
public class AdminOverrideHandler : AuthorizationHandler<ProUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProUserRequirement requirement)
    {
        if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
