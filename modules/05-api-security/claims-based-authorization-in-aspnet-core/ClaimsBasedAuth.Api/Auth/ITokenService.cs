using System.Security.Claims;
using ClaimsBasedAuth.Api.Entities;

namespace ClaimsBasedAuth.Api.Auth;

public interface ITokenService
{
    // Builds a signed JWT for the given user, their roles, and any stored user claims.
    (string Token, DateTime ExpiresAt) CreateToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        IEnumerable<Claim> userClaims);
}
