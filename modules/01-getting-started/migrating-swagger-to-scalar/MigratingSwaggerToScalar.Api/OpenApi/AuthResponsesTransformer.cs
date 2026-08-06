using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MigratingSwaggerToScalar.Api.OpenApi;

/// <summary>
/// Documents a standard 401 response on every secured endpoint. This is the native replacement
/// for a Swashbuckle IOperationFilter - it runs once per operation.
/// </summary>
internal sealed class AuthResponsesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (requiresAuth)
        {
            operation.Responses ??= new OpenApiResponses();
            operation.Responses["401"] = new OpenApiResponse { Description = "Unauthorized" };
        }

        return Task.CompletedTask;
    }
}
