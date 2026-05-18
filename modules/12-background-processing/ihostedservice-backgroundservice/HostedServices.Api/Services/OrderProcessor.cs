using HostedServices.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HostedServices.Api.Services;

/// <summary>
/// Demonstrates the two most important BackgroundService production patterns:
///
///   Gotcha 2 (captive dependency): BackgroundService is registered as a
///     singleton. Injecting DbContext (scoped) directly into the constructor
///     creates a captive dependency that lives for the host lifetime. Fix:
///     inject IServiceScopeFactory and create a fresh scope per iteration.
///
///   Gotcha 3 (silent service death): ExecuteAsync runs only as long as its
///     Task is alive. Returning from it - via an unhandled exception or an
///     accidental loop exit - marks the service as completed and it will
///     never run again. Fix: log on entry AND in a finally block on exit.
///     If the exit log appears outside a deploy window, the service has
///     died and you have minutes (not hours) to react.
/// </summary>
public sealed class OrderProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Service} ExecuteAsync starting", nameof(OrderProcessor));
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOneBatchAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Gotcha 1: do not let a transient error propagate up to the
                    // BackgroundService machinery, where StopHost (the .NET 6+
                    // default) would terminate the entire host process.
                    logger.LogError(ex, "Order processing iteration failed. Continuing.");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during graceful shutdown - do not log as error.
        }
        finally
        {
            // This is the alarm. If you see this log line at any time other
            // than during a deploy or scaling event, the service has died.
            logger.LogInformation("{Service} ExecuteAsync exiting", nameof(OrderProcessor));
        }
    }

    private async Task ProcessOneBatchAsync(CancellationToken ct)
    {
        // Gotcha 2 fix: fresh scope per iteration so DbContext is short-lived
        // and tracked entities, change detection, and any transient SQL errors
        // do not accumulate across the lifetime of the host.
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        var pending = await db.Orders
            .Where(o => !o.Processed)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            logger.LogDebug("No pending orders to process at {Time}", DateTimeOffset.UtcNow);
            return;
        }

        foreach (var order in pending)
        {
            order.Processed = true;
            logger.LogInformation("Processed order {OrderId} for {Customer} ({Total:C})",
                order.Id, order.Customer, order.Total);
        }

        await db.SaveChangesAsync(ct);
    }
}
