using System.Security.Claims;
using JwtAuthentication.Api.Entities;

namespace JwtAuthentication.Api.Endpoints;

public static class SecuredEndpoints
{
    public static void MapSecuredEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/secured").WithTags("Secured");

        // Any logged-in user with a valid token can reach this.
        group.MapGet("/", (ClaimsPrincipal user) =>
                Results.Ok($"Hello {user.Identity?.Name}, you reached a protected endpoint."))
            .RequireAuthorization();

        // Only users in the "Admin" role can reach this.
        group.MapGet("/admin", () =>
                Results.Ok("Hello Admin, this endpoint is for administrators only."))
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
    }
}
