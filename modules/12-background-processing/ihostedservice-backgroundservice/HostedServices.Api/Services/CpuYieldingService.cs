namespace HostedServices.Api.Services;

/// <summary>
/// Gotcha 5: CPU-bound work in ExecuteAsync holds a ThreadPool thread. With
/// enough background services doing this, you starve the thread pool and your
/// API request handlers start timing out for no obvious reason.
///
/// Two fixes:
///   1. await Task.Yield() between chunks so other thread-pool consumers can
///      run between batches of synchronous compute.
///   2. For genuinely heavy compute, push the work out of the API process
///      entirely (queue + dedicated worker process).
///
/// Note: the historical "await Task.Yield() at the top of ExecuteAsync to
/// prevent host startup from blocking" guidance no longer applies. Modern
/// BackgroundService (.NET 6+) wraps ExecuteAsync in Task.Run and returns
/// Task.CompletedTask from StartAsync unconditionally, so synchronous startup
/// work cannot block the host. The Task.Yield() calls below are about
/// thread-pool fairness during long-running compute, not host startup.
/// </summary>
public sealed class CpuYieldingService(ILogger<CpuYieldingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Service} ExecuteAsync starting", nameof(CpuYieldingService));
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var primeCount = await CountPrimesAsync(upperBound: 200_000, stoppingToken);
                    logger.LogInformation("Found {Count} primes in the current batch", primeCount);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Prime batch failed. Continuing.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during graceful shutdown.
        }
        finally
        {
            logger.LogInformation("{Service} ExecuteAsync exiting", nameof(CpuYieldingService));
        }
    }

    private static async Task<int> CountPrimesAsync(int upperBound, CancellationToken ct)
    {
        // Chunk the work and yield between chunks so we do not hold one
        // thread-pool thread for the full computation.
        const int chunkSize = 10_000;
        var primeCount = 0;

        for (var start = 2; start <= upperBound; start += chunkSize)
        {
            ct.ThrowIfCancellationRequested();

            var end = Math.Min(start + chunkSize, upperBound + 1);
            for (var n = start; n < end; n++)
            {
                if (IsPrime(n)) primeCount++;
            }

            // Hand the thread back to the pool between chunks.
            await Task.Yield();
        }

        return primeCount;
    }

    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n % 2 == 0) return n == 2;
        for (var i = 3; (long)i * i <= n; i += 2)
        {
            if (n % i == 0) return false;
        }
        return true;
    }
}
