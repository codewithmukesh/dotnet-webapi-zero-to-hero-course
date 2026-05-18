namespace HostedServices.Api.Services;

/// <summary>
/// The recommended shape: a BackgroundService with a single ExecuteAsync override.
/// The framework handles StartAsync, StopAsync, and shutdown cancellation for you.
/// Compare with HeartbeatHostedService.cs to see how much plumbing this saves.
/// </summary>
public sealed class HeartbeatBackgroundService(ILogger<HeartbeatBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Service} ExecuteAsync starting", nameof(HeartbeatBackgroundService));
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("BackgroundService heartbeat at {Time}", DateTimeOffset.UtcNow);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Heartbeat iteration failed. Continuing the loop.");
                }

                // Gotcha 4: pass stoppingToken so shutdown is immediate, not 5 seconds of waiting.
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during graceful shutdown.
        }
        finally
        {
            // Gotcha 3: exit log catches silent service deaths.
            logger.LogInformation("{Service} ExecuteAsync exiting", nameof(HeartbeatBackgroundService));
        }
    }
}
