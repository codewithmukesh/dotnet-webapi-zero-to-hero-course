using Microsoft.AspNetCore.Identity;

namespace CustomUserManagement.Api.Auth;

public static class IdentityResultExtensions
{
    // Turns Identity's error list into the shape Results.ValidationProblem expects, so a
    // failed CreateAsync/ResetPasswordAsync comes back as a clean RFC 9457 Problem Details body.
    public static Dictionary<string, string[]> ToProblemDictionary(this IdentityResult result) =>
        result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}
