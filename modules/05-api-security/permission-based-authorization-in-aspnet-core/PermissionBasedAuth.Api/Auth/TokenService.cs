using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Auth;

public class TokenService(
    IOptions<JwtSettings> jwtSettings,
    IOptions<PermissionSettings> permissionSettings) : ITokenService
{
    private readonly JwtSettings _settings = jwtSettings.Value;
    private readonly PermissionSettings _permissions = permissionSettings.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        IEnumerable<Claim> userClaims,
        IEnumerable<string> permissions)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        // Standard identity claims - who the user is.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Roles are claims too - one "role" claim per role. Always cheap: a user
        // has a handful of roles, not hundreds.
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        // Custom claims stored against the user in Identity.
        claims.AddRange(userClaims);

        // Only stamp permissions into the token when configured to. This is the
        // strategy that makes tokens grow - measure it at /api/diagnostics/token-size.
        if (_permissions.Source == PermissionSource.Token)
        {
            claims.AddRange(permissions.Select(p => new Claim(Permissions.ClaimType, p)));
        }

        return (BuildToken(claims, expiresAt), expiresAt);
    }

    // Shared by CreateToken and the diagnostics probe so the measured size comes
    // from the exact same signing path a real login uses.
    internal string BuildToken(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = credentials
        };

        // JsonWebTokenHandler is the modern, faster handler that replaces JwtSecurityTokenHandler.
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
