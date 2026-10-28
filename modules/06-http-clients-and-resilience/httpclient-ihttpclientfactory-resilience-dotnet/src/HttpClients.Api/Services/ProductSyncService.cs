using HttpClients.Api.Clients;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace HttpClients.Api.Services;

// A BackgroundService is a singleton, so it must not hold on to a typed or keyed client.
// It injects IHttpClientFactory and asks for a fresh HttpClient on every run instead.
public sealed class ProductSyncService(
    IHttpClientFactory httpClientFactory,
    ILogger<ProductSyncService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        do
        {
            try
            {
                var client = httpClientFactory.CreateClient("catalog");
                var products = await client.GetFromJsonAsync<List<ProductDto>>("products", stoppingToken);
                logger.LogInformation("Synced {Count} products from the catalog API", products?.Count ?? 0);
            }
            // An unhandled exception in a BackgroundService stops the host by default,
            // so catch the network error and the two exceptions the resilience pipeline can throw.
            catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or BrokenCircuitException)
            {
                logger.LogWarning(ex, "Product sync failed, will try again on the next tick");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
