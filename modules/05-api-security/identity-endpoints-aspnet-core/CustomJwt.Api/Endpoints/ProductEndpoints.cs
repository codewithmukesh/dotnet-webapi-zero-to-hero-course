namespace CustomJwt.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        // Same protected resource as the other project. The difference is the token:
        // here it is a real JWT, validated with the signing key, no opaque blob.
        app.MapGet("/api/products", () => new[]
        {
            new { Id = 1, Name = "Keyboard" },
            new { Id = 2, Name = "Monitor" }
        })
        .WithTags("Products")
        .RequireAuthorization();
    }
}
