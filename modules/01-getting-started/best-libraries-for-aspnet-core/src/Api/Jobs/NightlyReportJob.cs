using Api.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs;

public class NightlyReportJob(AppDbContext db, ILogger<NightlyReportJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-1);

        var orders = await db.Orders.CountAsync(o => o.CreatedAt >= since, ct);
        var revenue = await db.Orders
            .Where(o => o.CreatedAt >= since)
            .SumAsync(o => o.Total, ct);

        logger.LogInformation(
            "Nightly report: {OrderCount} orders worth {Revenue} in the last 24 hours",
            orders,
            revenue);
    }
}
