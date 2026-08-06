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

if (app.Environment.IsDevelopment())
{
    // --- Scalar UI (replaces UseSwaggerUI) ---
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Catalog API")
            .WithTheme(ScalarTheme.Mars)
            .WithPreferredScheme("Bearer")
            .AddHttpAuthentication("Bearer", auth =>
            {
                // Dev convenience only - never ship a real token to the browser.
                auth.Token = "paste-a-token-from-the-/token-endpoint";
            });
    });
}
else
{
    // Production: docs behind auth, external proxy disabled.
    app.MapOpenApi().RequireAuthorization();
    app.MapScalarApiReference(options => options.WithProxy(null)).RequireAuthorization();
}

var products = new List<Product>
{
    new(1, "Keyboard", 49.99m),
    new(2, "Mouse", 24.99m)
};

/// <summary>Issues a development JWT for testing secured endpoints.</summary>
app.MapPost("/token", (TokenRequest request) =>
    {
        var token = TokenService.CreateToken(request.Username, jwtKey, jwtIssuer);
        return Results.Ok(new { access_token = token });
    })
    .WithSummary("Get a development JWT")
    .WithTags("Auth");

/// <summary>Returns every product in the catalog.</summary>
app.MapGet("/products", () => Results.Ok(products))
    .WithSummary("List all products")
    .WithTags("Products");

/// <summary>Creates a new product. Requires a valid JWT.</summary>
app.MapPost("/products", (CreateProductRequest request) =>
    {
        var product = new Product(products.Count + 1, request.Name, request.Price);
        products.Add(product);
        return Results.Created($"/products/{product.Id}", product);
    })
    .WithSummary("Create a product")
    .WithTags("Products")
    .RequireAuthorization();

app.Run();
