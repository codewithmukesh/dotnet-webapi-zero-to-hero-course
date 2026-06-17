using Microsoft.AspNetCore.Authorization;

namespace PolicyBasedAuth.Api.Authorization;

// IAuthorizationRequirementData (.NET 8+) lets the ATTRIBUTE carry the requirement.
// No named policy registration needed - [MinimumTenure(6)] just works,
// with any number of months, without registering a policy per value.
public class MinimumTenureAttribute(int months) : AuthorizeAttribute, IAuthorizationRequirementData
{
    public int Months { get; } = months;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new MinimumTenureRequirement(Months);
    }
}

public class MinimumTenureRequirement(int months) : IAuthorizationRequirement
{
    public int Months { get; } = months;
}
