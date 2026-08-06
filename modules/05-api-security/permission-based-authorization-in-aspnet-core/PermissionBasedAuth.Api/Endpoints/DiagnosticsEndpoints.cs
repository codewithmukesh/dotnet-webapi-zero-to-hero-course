using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using PermissionBasedAuth.Api.Auth;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Endpoints;

// Measures what "just put the permissions in the token" actually costs.
// Anonymous on purpose: the numbers in the article should be reproducible by
// anyone who clones this repo without logging in first.
public static class DiagnosticsEndpoints
{
    // Kestrel's KestrelServerLimits.MaxRequestHeadersTotalSize default.
    private const int KestrelHeaderLimitBytes = 32_768;

    public static void MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diagnostics").WithTags("Diagnostics").AllowAnonymous();

        // Shows what the claims on the current caller ACTUALLY look like, including
        // the issuer on each one. Handy when a permission check silently denies.
        group.MapGet("/claims", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated is not true)
            {
                return Results.Ok(new { authenticated = false });
            }

            var identity = user.Identities.First();

            return Results.Ok(new
            {
                authenticated = true,
                roleClaimType = identity.RoleClaimType,
                nameClaimType = identity.NameClaimType,
                claims = user.Claims.Select(c => new { c.Type, c.Value, c.Issuer })
            });
        });

        group.MapGet("/token-size", (TokenService tokenService, int? permissions) =>
        {
            var counts = permissions is > 0 ? [permissions.Value] : new[] { 0, 10, 50, 200, 500 };

            var rows = counts.Select(count => Measure(tokenService, count));

            return Results.Ok(new
            {
                kestrelHeaderLimitBytes = KestrelHeaderLimitBytes,
                note = "headerBytes includes the 'Authorization: Bearer ' prefix and CRLF, "
                     + "which is what actually counts against the server header limit.",
                results = rows
            });
        });
    }

    private static object Measure(TokenService tokenService, int permissionCount)
    {
        // The same baseline claims a real login produces.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "admin@codewithmukesh.com"),
            new(JwtRegisteredClaimNames.Name, "Default Admin"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", Roles.Admin)
        };

        // Realistic permission strings: "Permissions.<Module>.<Action>".
        for (var i = 0; i < permissionCount; i++)
        {
            var module = $"Module{i / 4:D3}";
            var action = (i % 4) switch
            {
                0 => "View",
                1 => "Create",
                2 => "Edit",
                _ => "Delete"
            };
            claims.Add(new Claim(Permissions.ClaimType, $"{Permissions.Prefix}{module}.{action}"));
        }

        var token = tokenService.BuildToken(claims, DateTime.UtcNow.AddMinutes(60));

        var tokenBytes = Encoding.UTF8.GetByteCount(token);
        // "Authorization: Bearer " = 22 chars, CRLF = 2.
        var headerBytes = tokenBytes + 24;

        return new
        {
            permissions = permissionCount,
            tokenBytes,
            headerBytes,
            percentOfKestrelLimit = Math.Round(headerBytes * 100.0 / KestrelHeaderLimitBytes, 1),
            fitsInKestrelDefault = headerBytes <= KestrelHeaderLimitBytes
        };
    }
}
