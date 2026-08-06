using System.Collections.Concurrent;
using System.Threading.Channels;
using InvoicePdf.Api.Invoices;

namespace InvoicePdf.Api.Rendering;

public enum RenderState { Queued, Running, Done, Failed }

public sealed record RenderJob(Guid Id, Invoice Invoice);

public sealed record RenderResult(Guid Id, RenderState State, byte[]? Pdf, string? Error);

/// <summary>
/// A bounded work queue in front of PDF rendering.
///
/// The bound is the point. An unbounded queue turns a traffic spike into an
/// out-of-memory kill; a bounded one applies backpressure and lets the API
/// answer honestly that it is busy.
/// </summary>
public sealed class PdfRenderQueue
{
    private readonly Channel<RenderJob> _channel;
    private readonly ConcurrentDictionary<Guid, RenderResult> _results = new();

    public PdfRenderQueue(int capacity = 100)
    {
        _channel = Channel.CreateBounded<RenderJob>(new BoundedChannelOptions(capacity)
        {
            // Wait is what makes TryWrite return false once the channel is full,
            // which is what lets TryEnqueue report saturation. DropWrite would
            // return true and silently discard the job, handing the caller an id
            // for work that never happens.
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false,
        });
    }

    public IAsyncEnumerable<RenderJob> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);

    /// <summary>
    /// Returns null when the queue is saturated so the endpoint can answer 503.
    /// </summary>
    public Guid? TryEnqueue(Invoice invoice)
    {
        var job = new RenderJob(Guid.CreateVersion7(), invoice);

        if (!_channel.Writer.TryWrite(job))
        {
            return null;
        }

        _results[job.Id] = new RenderResult(job.Id, RenderState.Queued, null, null);
        return job.Id;
    }

    public void Update(RenderResult result) => _results[result.Id] = result;

    public RenderResult? Get(Guid id) => _results.TryGetValue(id, out var r) ? r : null;
}
