using Asp.Versioning;
using Asp.Versioning.Builder;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    // Advertise a removal date for v1 via an RFC 8594 Sunset header.
    options.Policies.Sunset(1.0)
        .Effective(new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero))
        .Link("https://api.example.com/docs/migrating-to-v2")
            .Title("Migration Guide")
            .Type("text/html");
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi();

var app = builder.Build();

ApiVersionSet versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder products = app
    .MapGroup("api/v{version:apiVersion}/products")
    .WithApiVersionSet(versionSet);

products.MapGet("/", () =>
        Results.Ok(new { version = "1.0", products = new[] { "Keyboard", "Mouse" } }))
    .MapToApiVersion(1.0);

products.MapGet("/", () =>
        Results.Ok(new { version = "2.0", data = new[] { "Keyboard", "Mouse" }, count = 2 }))
    .MapToApiVersion(2.0);

foreach (var description in app.DescribeApiVersions())
{
    app.MapOpenApi($"/openapi/{description.GroupName}.json");
}

app.MapScalarApiReference(options =>
{
    foreach (var description in app.DescribeApiVersions())
    {
        options.AddDocument(
            description.GroupName,
            description.GroupName,
            $"/openapi/{description.GroupName}.json");
    }
});

app.Run();
