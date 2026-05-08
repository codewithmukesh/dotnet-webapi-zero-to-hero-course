using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ApiKeyAuth.Api.Authentication;

internal sealed class ApiKeySecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [ApiKeyAuthenticationOptions.DefaultScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = ApiKeyAuthenticationOptions.HeaderName,
                In = ParameterLocation.Header,
                Description = $"API key sent in the {ApiKeyAuthenticationOptions.HeaderName} header."
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = schemes;

        if (document.Paths is null) return Task.CompletedTask;

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    ApiKeyAuthenticationOptions.DefaultScheme, document)] = []
            });
        }

        return Task.CompletedTask;
    }
}
