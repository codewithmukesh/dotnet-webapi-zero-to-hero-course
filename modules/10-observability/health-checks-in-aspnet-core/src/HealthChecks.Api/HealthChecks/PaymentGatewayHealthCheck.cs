using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Api.HealthChecks;

// A custom health check that measures the latency of a downstream dependency.
// It returns Degraded when the gateway is slow rather than failing outright,
// so a sluggish dependency surfaces in the dashboard without pulling the pod
// out of rotation.
public sealed class PaymentGatewayHealthCheck(IHttpClientFactory httpClientFactory)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("payments");
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            using var response = await client.GetAsync("/ping", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"Gateway returned {(int)response.StatusCode}");
            }

            var elapsed = Stopwatch.GetElapsedTime(startedAt);

            return elapsed.TotalMilliseconds > 800
                ? HealthCheckResult.Degraded(
                    $"Gateway slow: {elapsed.TotalMilliseconds:F0} ms")
                : HealthCheckResult.Healthy(
                    $"Gateway responded in {elapsed.TotalMilliseconds:F0} ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Gateway unreachable", ex);
        }
    }
}
