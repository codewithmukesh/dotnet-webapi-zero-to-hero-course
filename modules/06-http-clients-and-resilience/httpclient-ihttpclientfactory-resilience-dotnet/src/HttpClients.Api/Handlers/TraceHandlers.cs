using System.Collections.Concurrent;

namespace HttpClients.Api.Handlers;

// Collects what each handler saw, so the demo endpoint can return it.
public sealed class HandlerTrace
{
    private readonly ConcurrentQueue<string> _entries = new();

    public void Add(string entry) => _entries.Enqueue($"{DateTime.UtcNow:HH:mm:ss.fff} {entry}");

    public string[] Drain()
    {
        var entries = _entries.ToArray();
        _entries.Clear();
        return entries;
    }
}

// Registered BEFORE AddStandardResilienceHandler, so it sits outside the retry loop.
public sealed class OuterTraceHandler(HandlerTrace trace) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        trace.Add("outer handler: request started");
        var response = await base.SendAsync(request, cancellationToken);
        trace.Add($"outer handler: final response {(int)response.StatusCode}");
        return response;
    }
}

// Registered AFTER AddStandardResilienceHandler, so it runs once for every attempt.
public sealed class InnerTraceHandler(HandlerTrace trace) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        trace.Add($"inner handler: attempt returned {(int)response.StatusCode}");
        return response;
    }
}
