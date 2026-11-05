namespace HttpResilience.Handlers;

public sealed class CircuitOpenException(string message) : Exception(message);

// One instance per downstream service. It has to live outside the handler, because
// IHttpClientFactory rebuilds the handler chain every 2 minutes by default.
public sealed class CircuitBreakerState(int failureThreshold, TimeSpan breakDuration, TimeProvider time)
{
    private readonly Lock _lock = new();
    private int _failures;
    private DateTimeOffset? _openedAt;
    private bool _probeInFlight;

    public void EnsureCanSend()
    {
        lock (_lock)
        {
            if (_openedAt is null) return;
            if (time.GetUtcNow() - _openedAt.Value < breakDuration || _probeInFlight)
                throw new CircuitOpenException($"The circuit is open for {breakDuration.TotalSeconds}s.");
            _probeInFlight = true; // half-open: let one request test the service
        }
    }

    public void RecordSuccess() { lock (_lock) { _failures = 0; _openedAt = null; _probeInFlight = false; } }

    public void RecordFailure()
    {
        lock (_lock)
        {
            _probeInFlight = false;
            if (++_failures >= failureThreshold || _openedAt is not null) _openedAt = time.GetUtcNow();
        }
    }

    public void ReleaseProbe() { lock (_lock) _probeInFlight = false; }
}

public sealed class CircuitBreakerHandler(CircuitBreakerState state) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        state.EnsureCanSend();
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (TransientHttp.IsTransient(response)) state.RecordFailure();
            else state.RecordSuccess();
            return response;
        }
        catch (Exception ex) when (TransientHttp.IsTransient(ex))
        {
            state.RecordFailure();
            throw;
        }
        catch
        {
            state.ReleaseProbe();
            throw;
        }
    }
}
