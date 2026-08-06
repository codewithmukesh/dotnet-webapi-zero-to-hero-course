using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MigratingSwaggerToScalar.Api.OpenApi;

/// <summary>
/// Sets document-level metadata. This is the native replacement for the info you used to pass to
/// Swashbuckle's SwaggerDoc("v1", new OpenApiInfo { ... }).
/// </summary>
internal sealed class ApiInfoDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Title = "Catalog API";
        document.Info.Version = "v1";
        document.Info.Description = "Product catalog endpoints, migrated from Swagger to Scalar.";
        document.Info.Contact = new OpenApiContact
        {
            Name = "Mukesh Murugan",
            Url = new Uri("https://codewithmukesh.com")
        };
        return Task.CompletedTask;
    }
}
