using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EfCoreContainsLargeList.Shared;

public static class DbSetup
{
    public const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=EfCoreContainsLargeList;Trusted_Connection=True;TrustServerCertificate=True;Application Name=ContainsBench";

    public const int RowCount = 1_000_000;

    /// <summary>
    /// Creates the database and fills it with <see cref="RowCount"/> rows using SqlBulkCopy.
    /// Idempotent - if the table already holds the expected row count, it does nothing.
    /// </summary>
    public static async Task EnsureSeededAsync()
    {
        await using var context = new AppDbContext();
        await context.Database.EnsureCreatedAsync();

        var existing = await context.Products.CountAsync();
        if (existing >= RowCount)
        {
            Console.WriteLine($"Database already seeded with {existing:N0} rows.");
            return;
        }

        if (existing > 0)
        {
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Products]");
        }

        Console.WriteLine($"Seeding {RowCount:N0} rows...");
        var started = DateTime.UtcNow;

        using var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Sku", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Price", typeof(decimal));
        table.Columns.Add("CategoryId", typeof(int));

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Products",
            BatchSize = 50_000,
            BulkCopyTimeout = 0
        };

        foreach (var column in table.Columns.Cast<DataColumn>())
        {
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        for (var id = 1; id <= RowCount; id++)
        {
            table.Rows.Add(id, $"SKU-{id:D8}", $"Product {id}", 10m + id % 500, id % 50);

            if (table.Rows.Count == 50_000)
            {
                await bulk.WriteToServerAsync(table);
                table.Clear();
            }
        }

        if (table.Rows.Count > 0)
        {
            await bulk.WriteToServerAsync(table);
        }

        Console.WriteLine($"Seeded in {(DateTime.UtcNow - started).TotalSeconds:F1}s.");
    }

    /// <summary>Builds a list of ids spread across the table rather than a contiguous block.</summary>
    public static List<int> BuildIds(int count)
    {
        var step = Math.Max(1, RowCount / Math.Max(count, 1));
        var ids = new List<int>(count);
        for (var i = 0; i < count; i++)
        {
            ids.Add(i * step % RowCount + 1);
        }

        return ids;
    }
}
