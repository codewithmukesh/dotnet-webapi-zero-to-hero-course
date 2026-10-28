namespace HttpClients.Api.Clients;

public sealed record ProductDto(int Id, string Name, decimal Price);

// A small wrapper class around the keyed "catalog" client.
// IHttpClientFactory creates and configures the HttpClient, and keyed DI injects it here.
public sealed class CatalogClient([FromKeyedServices("catalog")] HttpClient httpClient)
{
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken)
    {
        var products = await httpClient.GetFromJsonAsync<List<ProductDto>>("products", cancellationToken);
        return products ?? [];
    }
}
