namespace IdentityEndpoints.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        // Anonymous request -> 401. With a valid cookie or bearer token -> 200.
        app.MapGet("/api/products", () => new[]
        {
            new { Id = 1, Name = "Keyboard" },
            new { Id = 2, Name = "Monitor" }
        })
        .WithTags("Products")
        .RequireAuthorization();
    }
}
