using HttpClients.Api.Options;
using Microsoft.Extensions.Options;

namespace HttpClients.Api.Handlers;

// Outgoing middleware. Every request sent through the client passes through SendAsync.
public sealed class ApiKeyHandler(IOptions<CatalogApiOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Remove first, so the header is never added twice to the same request.
        request.Headers.Remove("X-Api-Key");
        request.Headers.Add("X-Api-Key", options.Value.ApiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
