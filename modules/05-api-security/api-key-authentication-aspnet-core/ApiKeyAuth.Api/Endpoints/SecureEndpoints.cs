using System.Security.Claims;

namespace ApiKeyAuth.Api.Endpoints;

public static class SecureEndpoints
{
    public static IEndpointRouteBuilder MapSecureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "API Key Auth Demo - try /public, /secure, /admin/keys")
           .WithTags("Demo");

        app.MapGet("/public", () => new { message = "anyone can read this" })
           .WithTags("Demo");

        app.MapGet("/secure", (ClaimsPrincipal user) => new
        {
            message = "you are authenticated",
            client_id = user.FindFirst("client_id")?.Value,
            api_key_prefix = user.FindFirst("api_key_prefix")?.Value,
            scopes = user.FindAll("scope").Select(c => c.Value).ToArray()
        })
        .RequireAuthorization()
        .WithTags("Demo");

        return app;
    }
}
