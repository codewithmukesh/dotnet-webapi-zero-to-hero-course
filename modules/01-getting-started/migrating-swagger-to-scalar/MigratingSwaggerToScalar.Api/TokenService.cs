using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MigratingSwaggerToScalar.Api;

/// <summary>Issues short-lived development JWTs so secured endpoints can be tested in Scalar.</summary>
public static class TokenService
{
    public static string CreateToken(string username, string key, string issuer)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, username)]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credentials
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
