using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MigratingSwaggerToScalar.Api;
using MigratingSwaggerToScalar.Api.Models;
using MigratingSwaggerToScalar.Api.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Authentication (JWT Bearer) ---
// The security scheme transformer keys off the "Bearer" scheme registered here.
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// The document is regenerated on every request unless you cache it.
builder.Services.AddOutputCache();

// --- Native OpenAPI (replaces AddSwaggerGen) ---
// Every Swashbuckle filter and security definition ports to a transformer here.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<ApiInfoDocumentTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<AuthResponsesTransformer>();
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    // --- Scalar UI (replaces UseSwaggerUI) ---
    app.MapOpenApi().CacheOutput();
    app.MapOpenApi("/openapi/{documentName}.yaml").CacheOutput();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Catalog API")
            .WithTheme(ScalarTheme.Mars)
            .AddPreferredSecuritySchemes("Bearer")
            .AddHttpAuthentication("Bearer", auth =>
            {
                // Dev convenience only - never ship a real token to the browser.
                auth.Token = "paste-a-token-from-the-/token-endpoint";
            });
    });
}
else
{
    // Production: docs behind auth on BOTH endpoints - the JSON leaks the API surface too.
    // No proxy is configured, so the browser calls this API directly (Scalar only proxies
    // when WithProxyUrl is set explicitly).
    app.MapOpenApi().RequireAuthorization();
    app.MapScalarApiReference().RequireAuthorization();
}

// Inline lambdas cannot carry XML doc comments - a /// block above a MapPost call is a
// CS1587 warning and never reaches the document. Use the fluent methods instead.
app.MapPost("/token", (TokenRequest request) =>
    {
        var token = TokenService.CreateToken(request.Username, jwtKey, jwtIssuer);
        return Results.Ok(new { access_token = token });
    })
    .WithSummary("Get a development JWT")
    .WithDescription("Issues a short-lived JWT for testing the secured endpoints.")
    .WithName("GetToken")
    .WithTags("Auth");

// Named handler methods DO carry XML doc comments into the OpenAPI document.
app.MapGet("/products", ProductHandlers.List)
    .WithName("ListProducts")
    .WithTags("Products");

app.MapPost("/products", ProductHandlers.Create)
    .WithName("CreateProduct")
    .WithTags("Products")
    .RequireAuthorization();

app.Run();

/// <summary>Handlers for the product endpoints.</summary>
public static class ProductHandlers
{
    /// <summary>Returns every product in the catalog.</summary>
    public static IResult List() => Results.Ok(Products.All);

    /// <summary>Creates a new product in the catalog.</summary>
    /// <param name="request">The product details.</param>
    /// <returns>The created product with its generated id.</returns>
    public static IResult Create(CreateProductRequest request)
    {
        var product = new Product(Products.All.Count + 1, request.Name, request.Price);
        Products.All.Add(product);
        return Results.Created($"/products/{product.Id}", product);
    }
}

/// <summary>In-memory catalog store for the sample.</summary>
public static class Products
{
    /// <summary>The seeded product list.</summary>
    public static readonly List<Product> All =
    [
        new(1, "Keyboard", 49.99m),
        new(2, "Mouse", 24.99m)
    ];
}
