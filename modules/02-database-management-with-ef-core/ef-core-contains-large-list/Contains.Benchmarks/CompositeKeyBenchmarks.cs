using System.Data;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EfCoreContainsLargeList.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace EfCoreContainsLargeList.Benchmarks;

/// <summary>
/// The same question as <see cref="ContainsBenchmarks"/>, but the filter list is composite
/// keys - (TenantId, ProductId) pairs - against a one-million-row table.
///
/// Contains is not in this table because it does not compile down to anything: EF Core 10
/// cannot translate a Contains, an anonymous-type Contains, or an Any over a list of pairs.
/// What is left is an OR chain, a string key, or staging the pairs as a table.
/// </summary>
[SimpleJob(RunStrategy.Monitoring, iterationCount: 8, warmupCount: 2)]
[MemoryDiagnoser(displayGenColumns: false)]
public class CompositeKeyBenchmarks
{
    /// <summary>
    /// The largest OR chain that survives translation, measured by
    /// <c>Contains.Probe --composite-limit</c>. Past this the expression tree is deep enough
    /// that EF overflows the stack while walking it, which kills the process rather than
    /// throwing - so these benchmarks refuse to build one instead of taking the runner down.
    /// </summary>
    public const int OrChainCeiling = 489;

    private List<(int TenantId, int ProductId)> _pairs = [];
    private List<InventoryItem> _items = [];

    [Params(100, 400, 1_000, 5_000, 100_000)]
    public int ListSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        DbSetup.EnsureCompositeSeededAsync().GetAwaiter().GetResult();
        _pairs = DbSetup.BuildPairs(ListSize);
        _items = _pairs
            .Select(p => new InventoryItem { TenantId = p.TenantId, ProductId = p.ProductId })
            .ToList();
    }

    /// <summary>
    /// What a PredicateBuilder loop produces: one AND clause per pair, OR'd together, two
    /// parameters per pair. Returns -1 when the list is past the point where it can be run.
    /// </summary>
    [Benchmark]
    public int OrChain_Parameters()
    {
        if (ListSize > OrChainCeiling)
        {
            return -1;
        }

        using var context = new AppDbContext();
        try
        {
            return context.Inventory.AsNoTracking()
                .Where(CompositePredicates.BuildOrChain(_pairs, parameterized: true))
                .Select(i => i.TenantId)
                .ToList().Count;
        }
        catch (SqlException)
        {
            return -1;
        }
    }

    /// <summary>The same chain with the values inlined - no parameter budget, but the SQL text
    /// grows with the list and every distinct list leaves its own plan behind.</summary>
    [Benchmark]
    public int OrChain_Constants()
    {
        if (ListSize > OrChainCeiling)
        {
            return -1;
        }

        using var context = new AppDbContext();
        try
        {
            return context.Inventory.AsNoTracking()
                .Where(CompositePredicates.BuildOrChain(_pairs, parameterized: false))
                .Select(i => i.TenantId)
                .ToList().Count;
        }
        catch (SqlException)
        {
            return -1;
        }
    }

    /// <summary>
    /// The workaround that looks clever: glue the key parts into one string and use Contains.
    /// It translates, and it scans, because the concatenation is not sargable.
    /// </summary>
    [Benchmark]
    public int StringKey_Contains()
    {
        using var context = new AppDbContext();
        var keys = _pairs.Select(p => p.TenantId + "-" + p.ProductId).ToList();

        try
        {
            return context.Inventory.AsNoTracking()
                .Where(i => keys.Contains(i.TenantId + "-" + i.ProductId))
                .Select(i => i.TenantId)
                .ToList().Count;
        }
        catch (SqlException)
        {
            return -1;
        }
    }

    /// <summary>Hand-rolled: bulk copy the pairs into a two-column temp table, then join on both.</summary>
    [Benchmark(Baseline = true)]
    public int TempTable_Manual()
    {
        using var connection = new SqlConnection(DbSetup.ConnectionString);
        connection.Open();

        using (var create = connection.CreateCommand())
        {
            create.CommandText =
                "CREATE TABLE #Keys ([TenantId] int NOT NULL, [ProductId] int NOT NULL, " +
                "PRIMARY KEY ([TenantId], [ProductId]));";
            create.ExecuteNonQuery();
        }

        using var table = new DataTable();
        table.Columns.Add("TenantId", typeof(int));
        table.Columns.Add("ProductId", typeof(int));
        foreach (var (tenantId, productId) in _pairs)
        {
            table.Rows.Add(tenantId, productId);
        }

        using (var bulk = new SqlBulkCopy(connection) { DestinationTableName = "#Keys", BulkCopyTimeout = 0 })
        {
            bulk.ColumnMappings.Add("TenantId", "TenantId");
            bulk.ColumnMappings.Add("ProductId", "ProductId");
            bulk.WriteToServer(table);
        }

        var matched = new List<int>(_pairs.Count);
        using (var select = connection.CreateCommand())
        {
            select.CommandText =
                "SELECT i.[TenantId] FROM [Inventory] i INNER JOIN #Keys k " +
                "ON k.[TenantId] = i.[TenantId] AND k.[ProductId] = i.[ProductId];";
            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                matched.Add(reader.GetInt32(0));
            }
        }

        return matched.Count;
    }

    /// <summary>Entity Framework Extensions (commercial): the same temp table and join, but the
    /// query stays an IQueryable, and the key comes from the model rather than being spelled out.</summary>
    [Benchmark]
    public int WhereBulkContains_EFE()
    {
        using var context = new AppDbContext();
        return context.Inventory.AsNoTracking()
            .WhereBulkContains(_items)
            .Select(i => i.TenantId)
            .ToList().Count;
    }

    /// <summary>Entity Framework Plus (free): resolves the composite key to an inlined OR chain,
    /// so it inherits that approach's ceiling. Returns -1 when SQL Server refuses the query.</summary>
    [Benchmark]
    public int WhereContains_EFPlus()
    {
        if (ListSize > OrChainCeiling)
        {
            return -1;
        }

        using var context = new AppDbContext();
        try
        {
            return context.Inventory.AsNoTracking()
                .WhereContains(_items)
                .Select(i => i.TenantId)
                .ToList().Count;
        }
        catch (SqlException)
        {
            return -1;
        }
    }
}

/// <summary>Shared predicate construction so the probe and the benchmark measure the same thing.</summary>
public static class CompositePredicates
{
    private sealed class Box(int value)
    {
        public int Value { get; } = value;
    }

    public static Expression<Func<InventoryItem, bool>> BuildOrChain(
        IReadOnlyList<(int TenantId, int ProductId)> pairs,
        bool parameterized)
    {
        var item = Expression.Parameter(typeof(InventoryItem), "i");
        var tenant = Expression.Property(item, nameof(InventoryItem.TenantId));
        var product = Expression.Property(item, nameof(InventoryItem.ProductId));

        Expression Value(int value) => parameterized
            ? Expression.Property(Expression.Constant(new Box(value)), nameof(Box.Value))
            : Expression.Constant(value);

        Expression? body = null;
        foreach (var (tenantId, productId) in pairs)
        {
            var clause = Expression.AndAlso(
                Expression.Equal(tenant, Value(tenantId)),
                Expression.Equal(product, Value(productId)));

            body = body is null ? clause : Expression.OrElse(body, clause);
        }

        return Expression.Lambda<Func<InventoryItem, bool>>(
            body ?? Expression.Constant(false), item);
    }
}
