using Npgsql;

namespace CartesianExplosion.Measure;

public static class ResultSize
{
    public sealed record Statement(string Sql, long Rows, long Bytes);

    /// <summary>
    /// Re-runs a captured statement inside COUNT/SUM(pg_column_size) to get its row count and result size.
    /// "Result size" is Postgres's row size (including a per-row header), not exact wire bytes.
    /// </summary>
    public static async Task<Statement> MeasureAsync(string connectionString, string sql, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(
            $"SELECT count(*), coalesce(sum(pg_column_size(t.*)), 0)::bigint FROM ({sql}) AS t", connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new Statement(sql, reader.GetInt64(0), reader.GetInt64(1));
    }
}
