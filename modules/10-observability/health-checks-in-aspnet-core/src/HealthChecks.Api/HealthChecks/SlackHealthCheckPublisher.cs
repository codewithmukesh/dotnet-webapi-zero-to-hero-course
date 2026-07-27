using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Api.HealthChecks;

// Push instead of poll. The runtime runs the registered checks on a timer
// (see HealthCheckPublisherOptions in Program.cs) and hands the report here,
// so monitoring gets status without anyone hitting an HTTP endpoint.
//
// One timer per app, no matter how many systems want to know the status.
public sealed class SlackHealthCheckPublisher(
    ILogger<SlackHealthCheckPublisher> logger) : IHealthCheckPublisher
{
    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        if (report.Status == HealthStatus.Healthy)
        {
            logger.LogInformation(
                "Health report: {Status} in {DurationMs:F0} ms",
                report.Status,
                report.TotalDuration.TotalMilliseconds);

            return Task.CompletedTask;
        }

        // Only the failing entries are worth paging on.
        var failing = report.Entries
            .Where(entry => entry.Value.Status != HealthStatus.Healthy)
            .Select(entry => $"{entry.Key}={entry.Value.Status} ({entry.Value.Description})");

        logger.LogWarning(
            "Health report: {Status}. Failing checks: {Failing}",
            report.Status,
            string.Join(", ", failing));

        // Replace this with the POST to your Slack incoming webhook, PagerDuty
        // event API, or status page. Honour the cancellation token when you do.
        return Task.CompletedTask;
    }
}
