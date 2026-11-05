namespace HttpResilience.Handlers;

public sealed class RetryHandler(int maxRetries, TimeSpan baseDelay, TimeSpan maxDelay) : DelegatingHandler
{
    private static readonly HashSet<HttpMethod> IdempotentMethods =
        [HttpMethod.Get, HttpMethod.Head, HttpMethod.Options, HttpMethod.Put, HttpMethod.Delete];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!IdempotentMethods.Contains(request.Method))
            return await base.SendAsync(request, cancellationToken);

        for (var attempt = 0; ; attempt++)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
                if (!TransientHttp.IsTransient(response) || attempt == maxRetries) return response;
            }
            catch (Exception ex) when (TransientHttp.IsTransient(ex) && attempt < maxRetries
                                       && !cancellationToken.IsCancellationRequested)
            {
            }

            var delay = GetDelay(response, attempt);
            response?.Dispose();
            await Task.Delay(delay, cancellationToken);
        }
    }

    private TimeSpan GetDelay(HttpResponseMessage? response, int attempt)
    {
        var retryAfter = response?.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta) return Min(delta, maxDelay);
        if (retryAfter?.Date is { } date)
        {
            var wait = date - DateTimeOffset.UtcNow;
            return wait <= TimeSpan.Zero ? TimeSpan.Zero : Min(wait, maxDelay);
        }
        var exponential = baseDelay * Math.Pow(2, attempt);
        return Min(exponential * (0.5 + Random.Shared.NextDouble()), maxDelay);
    }

    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;
}
