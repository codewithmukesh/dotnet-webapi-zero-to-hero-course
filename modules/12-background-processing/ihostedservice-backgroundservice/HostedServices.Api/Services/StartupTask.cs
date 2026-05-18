namespace HostedServices.Api.Services;

/// <summary>
/// One-shot startup work: implement IHostedService directly when the work
/// does not fit the continuous-loop shape that BackgroundService is designed
/// for. Typical uses: warm a cache, seed reference data, register with a
/// service discovery system, run an idempotent migration check.
///
/// The host blocks on StartAsync, so do not do anything that takes longer
/// than your readiness probe timeout. If it is genuinely slow, fire it off
/// with Task.Run inside StartAsync and return Task.CompletedTask.
/// </summary>
public sealed class StartupTask(ILogger<StartupTask> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("StartupTask running at {Time}. Doing one-shot init...", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("StartupTask shutting down at {Time}.", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
