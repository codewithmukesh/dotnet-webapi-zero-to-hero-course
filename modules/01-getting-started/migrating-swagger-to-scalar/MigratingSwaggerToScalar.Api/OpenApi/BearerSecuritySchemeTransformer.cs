using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MigratingSwaggerToScalar.Api.OpenApi;

/// <summary>
/// Adds the JWT Bearer security scheme to the OpenAPI document so Scalar renders the authorize lock.
/// This is the native replacement for AddSecurityDefinition + AddSecurityRequirement in Swashbuckle.
/// Scalar reads security schemes from the document, so AddPreferredSecuritySchemes("Bearer") does nothing
/// without this transformer.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider schemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        if (!schemes.Any(s => s.Name == "Bearer"))
        {
            return;
        }

        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste a JWT access token. No 'Bearer' prefix needed."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = bearerScheme;

        // Reference the scheme by id so every operation shows the lock in Scalar.
        var reference = new OpenApiSecuritySchemeReference("Bearer", document);
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [reference] = []
        });
    }
}
