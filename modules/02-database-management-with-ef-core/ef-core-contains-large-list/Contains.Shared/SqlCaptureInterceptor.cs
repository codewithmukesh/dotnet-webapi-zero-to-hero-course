using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreContainsLargeList.Shared;

/// <summary>
/// Captures the command EF actually sends to SQL Server. The interceptor sees the real
/// DbCommand, so inlined constants are visible here even though EF 10 redacts them in logs.
/// </summary>
public sealed class SqlCaptureInterceptor : DbCommandInterceptor
{
    public string? LastSql { get; private set; }
    public int LastParameterCount { get; private set; }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Capture(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Capture(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void Capture(DbCommand command)
    {
        LastSql = command.CommandText;
        LastParameterCount = command.Parameters.Count;
    }
}
