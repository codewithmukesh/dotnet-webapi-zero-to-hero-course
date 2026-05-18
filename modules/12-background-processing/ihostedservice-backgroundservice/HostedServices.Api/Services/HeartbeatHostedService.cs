namespace HostedServices.Api.Services;

/// <summary>
/// Same behaviour as HeartbeatBackgroundService, but implemented against the
/// raw IHostedService interface so you can see exactly how much lifecycle
/// plumbing BackgroundService is doing for you. In real code, prefer
/// BackgroundService for loop work — this exists only to illustrate the gap.
/// </summary>
public sealed class HeartbeatHostedService(ILogger<HeartbeatHostedService> logger) : IHostedService, IAsyncDisposable
{
    private Task? _backgroundTask;
    private CancellationTokenSource? _stoppingCts;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = RunAsync(_stoppingCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_backgroundTask is null) return;

        try
        {
            _stoppingCts!.Cancel();
        }
        finally
        {
            await Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("IHostedService heartbeat at {Time}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Heartbeat iteration failed. Continuing the loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stoppingCts?.Cancel();
        if (_backgroundTask is not null)
        {
            await _backgroundTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        _stoppingCts?.Dispose();
    }
}
