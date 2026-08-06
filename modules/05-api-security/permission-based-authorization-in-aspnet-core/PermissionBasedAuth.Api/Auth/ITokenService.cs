using System.Security.Claims;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Auth;

public interface ITokenService
{
    // Builds a signed JWT for the given user, their roles, and any stored user claims.
    // `permissions` is only embedded when PermissionSettings.Source is Token.
    (string Token, DateTime ExpiresAt) CreateToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        IEnumerable<Claim> userClaims,
        IEnumerable<string> permissions);
}
