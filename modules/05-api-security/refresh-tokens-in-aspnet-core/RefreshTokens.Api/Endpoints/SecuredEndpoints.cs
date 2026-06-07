using System.Security.Claims;

namespace RefreshTokens.Api.Endpoints;

public static class SecuredEndpoints
{
    public static void MapSecuredEndpoints(this IEndpointRouteBuilder app)
    {
        // A protected endpoint to prove the access token works.
        app.MapGet("/api/secured", (ClaimsPrincipal user) =>
                Results.Ok($"Hello {user.Identity?.Name}, your access token is valid."))
            .RequireAuthorization()
            .WithTags("Secured");
    }
}
