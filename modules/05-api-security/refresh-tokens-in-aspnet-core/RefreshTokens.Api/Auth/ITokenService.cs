using RefreshTokens.Api.Entities;

namespace RefreshTokens.Api.Auth;

public interface ITokenService
{
    // Short-lived JWT the client sends on every request.
    (string Token, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    // Long-lived random string the client uses to get a new access token.
    string CreateRefreshToken();
}
