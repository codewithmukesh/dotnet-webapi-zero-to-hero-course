using Microsoft.AspNetCore.Authorization;

namespace PolicyBasedAuth.Api.Authorization;

// Handlers are resolved from DI, so they can take dependencies - here TimeProvider,
// which makes the rule unit-testable with a fake clock.
public class WorkingHoursHandler(TimeProvider timeProvider) : AuthorizationHandler<WorkingHoursRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkingHoursRequirement requirement)
    {
        var hour = timeProvider.GetUtcNow().Hour;

        if (hour >= requirement.StartHourUtc && hour < requirement.EndHourUtc)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
