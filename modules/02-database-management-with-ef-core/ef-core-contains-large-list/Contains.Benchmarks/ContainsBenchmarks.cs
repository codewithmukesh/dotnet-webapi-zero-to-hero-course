using System.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using EfCoreContainsLargeList.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace EfCoreContainsLargeList.Benchmarks;

[SimpleJob(RunStrategy.Monitoring, iterationCount: 8, warmupCount: 2)]
[MemoryDiagnoser(displayGenColumns: false)]
public class ContainsBenchmarks
{
    private List<int> _ids = [];

    [Params(10, 100, 1_000, 2_098, 2_099, 5_000, 10_000, 100_000)]
    public int ListSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        DbSetup.EnsureSeededAsync().GetAwaiter().GetResult();
        _ids = DbSetup.BuildIds(ListSize);
    }

    /// <summary>EF Core 10 default. Multiple scalar parameters, falling back to OPENJSON past 2,098.</summary>
    [Benchmark(Baseline = true)]
    public int Contains_Default()
    {
        using var context = new AppDbContext();
        return context.Products.AsNoTracking()
            .Where(p => _ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToList().Count;
    }

    /// <summary>Single JSON array parameter - the EF Core 8/9 default.</summary>
    [Benchmark]
    public int Contains_EfParameter()
    {
        using var context = new AppDbContext();
        var ids = _ids;
        return context.Products.AsNoTracking()
            .Where(p => EF.Parameter(ids).Contains(p.Id))
            .Select(p => p.Id)
            .ToList().Count;
    }

    /// <summary>
    /// Values inlined as constants - the pre-EF8 default. Returns -1 when SQL Server refuses
    /// the query; at large list sizes this fails with "The query processor ran out of internal
    /// resources", which is a different failure from the 2,100 parameter error.
    /// </summary>
    [Benchmark]
    public int Contains_EfConstant()
    {
        using var context = new AppDbContext();
        var ids = _ids;
        try
        {
            return context.Products.AsNoTracking()
                .Where(p => EF.Constant(ids).Contains(p.Id))
                .Select(p => p.Id)
                .ToList().Count;
        }
        catch (SqlException)
        {
            return -1;
        }
    }

    /// <summary>The folk remedy: split the list into sub-2,000 batches and union client-side.</summary>
    [Benchmark]
    public int Contains_Chunked()
    {
        using var context = new AppDbContext();
        var matched = new List<int>(_ids.Count);

        foreach (var chunk in _ids.Chunk(2_000))
        {
            var batch = chunk.ToList();
            matched.AddRange(context.Products.AsNoTracking()
                .Where(p => batch.Contains(p.Id))
                .Select(p => p.Id));
        }

        return matched.Count;
    }

    /// <summary>Hand-rolled: bulk copy the ids into a temp table, then INNER JOIN.</summary>
    [Benchmark]
    public int TempTable_Manual()
    {
        using var connection = new SqlConnection(DbSetup.ConnectionString);
        connection.Open();

        using (var create = connection.CreateCommand())
        {
            create.CommandText = "CREATE TABLE #Ids ([Value] int NOT NULL PRIMARY KEY);";
            create.ExecuteNonQuery();
        }

        using var table = new DataTable();
        table.Columns.Add("Value", typeof(int));
        foreach (var id in _ids)
        {
            table.Rows.Add(id);
        }

        using (var bulk = new SqlBulkCopy(connection) { DestinationTableName = "#Ids", BulkCopyTimeout = 0 })
        {
            bulk.ColumnMappings.Add("Value", "Value");
            bulk.WriteToServer(table);
        }

        var matched = new List<int>(_ids.Count);
        using (var select = connection.CreateCommand())
        {
            select.CommandText =
                "SELECT p.[Id] FROM [Products] p INNER JOIN #Ids i ON i.[Value] = p.[Id];";
            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                matched.Add(reader.GetInt32(0));
            }
        }

        return matched.Count;
    }

    /// <summary>Entity Framework Extensions (commercial): temp table + BulkInsert + INNER JOIN.</summary>
    [Benchmark]
    public int WhereBulkContains_EFE()
    {
        using var context = new AppDbContext();
        return context.Products.AsNoTracking()
            .WhereBulkContains(_ids)
            .Select(p => p.Id)
            .ToList().Count;
    }

    /// <summary>
    /// Entity Framework Plus (free): picks Contains, Any, or the paid bulk path. Returns -1
    /// when SQL Server refuses the query - without a licence the bulk path is unavailable,
    /// so it inherits the limits of whichever built-in resolution it lands on.
    /// </summary>
    [Benchmark]
    public int WhereContains_EFPlus()
    {
        using var context = new AppDbContext();
        try
        {
            return context.Products.AsNoTracking()
                .WhereContains(_ids, p => p.Id)
                .Select(p => p.Id)
                .ToList().Count;
        }
        catch (SqlException)
        {
            return -1;
        }
    }
}
