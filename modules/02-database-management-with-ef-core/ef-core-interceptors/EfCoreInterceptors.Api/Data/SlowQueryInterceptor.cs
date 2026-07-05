using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreInterceptors.Api.Data;

// Logs any SQL command that takes longer than the threshold. Stateless, so it
// is safe to register as a singleton (ILogger is a singleton-friendly service).
public sealed class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger)
    : DbCommandInterceptor
{
    private const int SlowQueryThresholdMs = 500;

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Log(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Log(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void Log(DbCommand command, CommandExecutedEventData eventData)
    {
        if (eventData.Duration.TotalMilliseconds >= SlowQueryThresholdMs)
        {
            logger.LogWarning(
                "Slow query took {ElapsedMs}ms: {Sql}",
                eventData.Duration.TotalMilliseconds,
                command.CommandText);
        }
    }
}
