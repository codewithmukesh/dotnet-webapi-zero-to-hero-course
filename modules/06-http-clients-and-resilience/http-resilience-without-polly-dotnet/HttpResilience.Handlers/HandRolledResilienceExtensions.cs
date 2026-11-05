using Microsoft.Extensions.DependencyInjection;

namespace HttpResilience.Handlers;

public sealed class HandRolledResilienceOptions
{
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public int MaxRetries { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(2);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(5);
}

public static class HandRolledResilienceExtensions
{
    public static IHttpClientBuilder AddHandRolledResilience(
        this IHttpClientBuilder builder, Action<HandRolledResilienceOptions>? configure = null)
    {
        var options = new HandRolledResilienceOptions();
        configure?.Invoke(options);

        // Created once per named client, so the breaker state survives handler rotation.
        var breaker = new CircuitBreakerState(options.FailureThreshold, options.BreakDuration, TimeProvider.System);

        return builder
            .ConfigureHttpClient(client => client.Timeout = options.TotalTimeout)
            .AddHttpMessageHandler(() => new RetryHandler(options.MaxRetries, options.BaseDelay, options.MaxDelay))
            .AddHttpMessageHandler(() => new CircuitBreakerHandler(breaker))
            .AddHttpMessageHandler(() => new AttemptTimeoutHandler(options.AttemptTimeout));
    }
}
