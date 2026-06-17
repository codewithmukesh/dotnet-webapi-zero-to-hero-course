using Microsoft.AspNetCore.Authorization;

namespace PolicyBasedAuth.Api.Authorization;

// Requirements carry DATA for the rule. This one says: only between these hours (UTC).
public class WorkingHoursRequirement(int startHourUtc, int endHourUtc) : IAuthorizationRequirement
{
    public int StartHourUtc { get; } = startHourUtc;
    public int EndHourUtc { get; } = endHourUtc;
}
