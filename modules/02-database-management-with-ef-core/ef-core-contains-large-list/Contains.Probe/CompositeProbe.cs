using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using EfCoreContainsLargeList.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace EfCoreContainsLargeList.Probe;

/// <summary>
/// Finds out what EF Core 10 will and will not translate when the filter list is a set of
/// composite keys rather than scalars.
///
/// Every approach runs in its own child process. An OR chain built from enough pairs
/// overflows the stack while EF walks the expression tree, and a stack overflow cannot be
/// caught - it takes the process down. Running in-process would lose every later result.
/// </summary>
public static class CompositeProbe
{
    private static readonly string[] Approaches =
    [
        "valuetuple",
        "anonymous",
        "any",
        "orchain-param",
        "orchain-const",
        "orchain-balanced",
        "stringkey",
        "efe-bulk",
        "efplus",
        "temptable"
    ];

    /// <summary>Holds a value so an expression tree reads it as a captured variable, which is
    /// what EF parameterizes. Expression.Constant would be inlined as a literal instead.</summary>
    private sealed class Box(int value)
    {
        public int Value { get; } = value;
    }

    /// <summary>Runs every approach at one list size, each in its own process.</summary>
    public static async Task RunAsync(int size)
    {
        await DbSetup.EnsureCompositeSeededAsync();

        Console.WriteLine($"## Composite key probe - {size:N0} (TenantId, ProductId) pairs");
        Console.WriteLine();

        ReportEfPlusSurface();

        foreach (var approach in Approaches)
        {
            var (output, exitCode) = await RunChildAsync(approach, size);

            if (exitCode == 0)
            {
                Console.Write(output);
            }
            else
            {
                Console.WriteLine($"{approach,-36} {"-",8}     PROCESS DIED exit={exitCode}");
                Console.WriteLine("   stack overflow while EF walked the expression tree - uncatchable");
                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// Bisects the largest pair count an OR chain survives, running each trial in its own
    /// process so a stack overflow is a data point rather than the end of the run.
    /// </summary>
    public static async Task FindOrChainLimitAsync(string approach, int low, int high)
    {
        await DbSetup.EnsureCompositeSeededAsync();
        Console.WriteLine($"## Bisecting the {approach} ceiling between {low:N0} and {high:N0} pairs");
        Console.WriteLine();

        var lastGood = 0;
        var firstBad = high + 1;

        while (low <= high)
        {
            var mid = low + (high - low) / 2;
            var (output, exitCode) = await RunChildAsync(approach, mid);
            var verdict = exitCode == 0
                ? (output.Contains("FAILED") ? "threw" : "ok")
                : $"process died (exit {exitCode})";

            Console.WriteLine($"  {mid,8:N0} pairs -> {verdict}");

            if (verdict == "ok")
            {
                lastGood = mid;
                low = mid + 1;
            }
            else
            {
                firstBad = mid;
                high = mid - 1;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"  Largest surviving list: {lastGood:N0} pairs ({lastGood * 2:N0} parameters)");
        Console.WriteLine($"  First failing list:     {firstBad:N0} pairs ({firstBad * 2:N0} parameters)");
        Console.WriteLine();
    }

    private static async Task<(string Output, int ExitCode)> RunChildAsync(string approach, int size)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath!,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Environment.ProcessPath is the apphost, so the assembly path is not needed as an argument.
        startInfo.ArgumentList.Add("--composite-one");
        startInfo.ArgumentList.Add(approach);
        startInfo.ArgumentList.Add(size.ToString());

        using var process = Process.Start(startInfo)!;

        // Both pipes have to be drained at the same time. A stack overflow writes a long
        // message to stderr, and reading stdout to the end first deadlocks against it.
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            return ("   timed out after 5 minutes\n", -1);
        }

        await Task.WhenAll(stdout, stderr);

        return (stdout.Result + stderr.Result, process.ExitCode);
    }

    /// <summary>Runs exactly one approach. This is what the child process executes.</summary>
    public static async Task RunOneAsync(string approach, int size)
    {
        await DbSetup.EnsureCompositeSeededAsync(quiet: true);
        var pairs = DbSetup.BuildPairs(size);

        switch (approach)
        {
            case "valuetuple":
                await Probe("ValueTuple Contains", size, ctx =>
                {
                    var keys = pairs.ToList();
                    return ctx.Inventory.AsNoTracking()
                        .Where(i => keys.Contains(new ValueTuple<int, int>(i.TenantId, i.ProductId)))
                        .Select(i => i.TenantId).ToList().Count;
                });
                break;

            case "anonymous":
                await Probe("Anonymous type Contains", size, ctx =>
                {
                    var keys = pairs.Select(p => new { p.TenantId, p.ProductId }).ToList();
                    return ctx.Inventory.AsNoTracking()
                        .Where(i => keys.Contains(new { i.TenantId, i.ProductId }))
                        .Select(i => i.TenantId).ToList().Count;
                });
                break;

            case "any":
                await Probe("Any over the pair list", size, ctx =>
                {
                    var keys = pairs.ToList();
                    return ctx.Inventory.AsNoTracking()
                        .Where(i => keys.Any(k => k.TenantId == i.TenantId && k.ProductId == i.ProductId))
                        .Select(i => i.TenantId).ToList().Count;
                });
                break;

            case "orchain-param":
                await Probe("OR chain (parameters)", size, ctx =>
                    ctx.Inventory.AsNoTracking()
                        .Where(BuildOrChain(pairs, parameterized: true, balanced: false))
                        .Select(i => i.TenantId).ToList().Count);
                break;

            case "orchain-const":
                await Probe("OR chain (constants)", size, ctx =>
                    ctx.Inventory.AsNoTracking()
                        .Where(BuildOrChain(pairs, parameterized: false, balanced: false))
                        .Select(i => i.TenantId).ToList().Count);
                break;

            case "orchain-balanced":
                await Probe("OR chain (parameters, balanced)", size, ctx =>
                    ctx.Inventory.AsNoTracking()
                        .Where(BuildOrChain(pairs, parameterized: true, balanced: true))
                        .Select(i => i.TenantId).ToList().Count);
                break;

            case "stringkey":
                await Probe("String key Contains", size, ctx =>
                {
                    var keys = pairs.Select(p => p.TenantId + "-" + p.ProductId).ToList();
                    return ctx.Inventory.AsNoTracking()
                        .Where(i => keys.Contains(i.TenantId + "-" + i.ProductId))
                        .Select(i => i.TenantId).ToList().Count;
                });
                break;

            case "efe-bulk":
                await Probe("WhereBulkContains (EFE)", size, ctx =>
                    ctx.Inventory.AsNoTracking()
                        .WhereBulkContains(ToItems(pairs))
                        .Select(i => i.TenantId).ToList().Count);
                break;

            case "efplus":
                await Probe("WhereContains (EF Plus)", size, ctx =>
                    ctx.Inventory.AsNoTracking()
                        .WhereContains(ToItems(pairs))
                        .Select(i => i.TenantId).ToList().Count);
                break;

            case "temptable":
                ProbeManualTempTable(size, pairs);
                break;

            default:
                Console.WriteLine($"unknown approach '{approach}'");
                break;
        }
    }

    private static List<InventoryItem> ToItems(IReadOnlyList<(int TenantId, int ProductId)> pairs) =>
        pairs.Select(p => new InventoryItem { TenantId = p.TenantId, ProductId = p.ProductId }).ToList();

    /// <summary>Lists the WhereContains overloads on the classpath and, importantly, which
    /// assembly each one lives in - so the composite-key claim is checked against the binary.</summary>
    private static void ReportEfPlusSurface()
    {
        string[] assemblyNames =
        [
            "Z.EntityFramework.Plus.EFCore",
            "Z.EntityFramework.Extensions.EFCore"
        ];

        var assemblies = new List<Assembly>();
        foreach (var name in assemblyNames)
        {
            try
            {
                assemblies.Add(Assembly.Load(name));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  (could not load {name}: {ex.GetType().Name})");
            }
        }

        var overloads = assemblies
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.Name == "WhereContains")
            .Select(m =>
                $"  [{m.DeclaringType!.Assembly.GetName().Name}] " +
                "WhereContains<" +
                string.Join(", ", m.GetGenericArguments().Select(a => a.Name)) +
                ">(" +
                string.Join(", ", m.GetParameters().Select(p => Pretty(p.ParameterType))) +
                ")")
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        Console.WriteLine($"WhereContains overloads on the classpath ({overloads.Count}):");
        foreach (var overload in overloads)
        {
            Console.WriteLine(overload);
        }

        Console.WriteLine();
    }

    private static string Pretty(Type type) =>
        type.IsGenericType
            ? Strip(type.Name) + "<" + string.Join(", ", type.GetGenericArguments().Select(Pretty)) + ">"
            : type.Name;

    private static string Strip(string name)
    {
        var tick = name.IndexOf('`');
        return tick < 0 ? name : name[..tick];
    }

    /// <summary>
    /// Builds the predicate a PredicateBuilder loop produces: one AND clause per pair, OR'd
    /// together. <paramref name="balanced"/> combines them as a balanced tree instead of the
    /// left-deep chain a foreach loop produces, to test whether depth alone is the problem.
    /// </summary>
    private static Expression<Func<InventoryItem, bool>> BuildOrChain(
        IReadOnlyList<(int TenantId, int ProductId)> pairs,
        bool parameterized,
        bool balanced)
    {
        var item = Expression.Parameter(typeof(InventoryItem), "i");
        var tenant = Expression.Property(item, nameof(InventoryItem.TenantId));
        var product = Expression.Property(item, nameof(InventoryItem.ProductId));

        Expression Value(int value) => parameterized
            ? Expression.Property(Expression.Constant(new Box(value)), nameof(Box.Value))
            : Expression.Constant(value);

        var clauses = pairs
            .Select(pair => (Expression)Expression.AndAlso(
                Expression.Equal(tenant, Value(pair.TenantId)),
                Expression.Equal(product, Value(pair.ProductId))))
            .ToList();

        if (clauses.Count == 0)
        {
            return Expression.Lambda<Func<InventoryItem, bool>>(Expression.Constant(false), item);
        }

        Expression body;
        if (balanced)
        {
            while (clauses.Count > 1)
            {
                var merged = new List<Expression>((clauses.Count + 1) / 2);
                for (var i = 0; i < clauses.Count; i += 2)
                {
                    merged.Add(i + 1 < clauses.Count
                        ? Expression.OrElse(clauses[i], clauses[i + 1])
                        : clauses[i]);
                }

                clauses = merged;
            }

            body = clauses[0];
        }
        else
        {
            body = clauses[0];
            for (var i = 1; i < clauses.Count; i++)
            {
                body = Expression.OrElse(body, clauses[i]);
            }
        }

        return Expression.Lambda<Func<InventoryItem, bool>>(body, item);
    }

    private static async Task Probe(string name, int expected, Func<AppDbContext, int> run)
    {
        var capture = new SqlCaptureInterceptor();
        await using var context = new AppDbContext(capture: capture);

        try
        {
            var started = Stopwatch.StartNew();
            var count = run(context);
            started.Stop();
            var verdict = count == expected ? "ok" : $"MISMATCH got {count:N0}";

            Console.WriteLine(
                $"{name,-36} {started.ElapsedMilliseconds,8:N0} ms  params={capture.LastParameterCount,-6} {verdict}");
            Console.WriteLine($"   sql len={capture.LastSql?.Length ?? 0:N0}: {Shape(capture.LastSql)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name,-36} {"-",8}     FAILED {ex.GetType().Name}");
            Console.WriteLine($"   {Flatten(ex.Message)}");
        }

        Console.WriteLine();
    }

    private static void ProbeManualTempTable(int expected, IReadOnlyList<(int TenantId, int ProductId)> pairs)
    {
        try
        {
            var started = Stopwatch.StartNew();
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
            foreach (var (tenantId, productId) in pairs)
            {
                table.Rows.Add(tenantId, productId);
            }

            using (var bulk = new SqlBulkCopy(connection) { DestinationTableName = "#Keys", BulkCopyTimeout = 0 })
            {
                bulk.ColumnMappings.Add("TenantId", "TenantId");
                bulk.ColumnMappings.Add("ProductId", "ProductId");
                bulk.WriteToServer(table);
            }

            int matched;
            using (var select = connection.CreateCommand())
            {
                select.CommandText =
                    "SELECT COUNT(*) FROM [Inventory] i INNER JOIN #Keys k " +
                    "ON k.[TenantId] = i.[TenantId] AND k.[ProductId] = i.[ProductId];";
                matched = (int)select.ExecuteScalar()!;
            }

            started.Stop();
            var verdict = matched == expected ? "ok" : $"MISMATCH got {matched:N0}";
            Console.WriteLine(
                $"{"Temp table + join (manual)",-36} {started.ElapsedMilliseconds,8:N0} ms  params=0      {verdict}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{"Temp table + join (manual)",-36} {"-",8}     FAILED {ex.GetType().Name}");
            Console.WriteLine($"   {Flatten(ex.Message)}");
        }

        Console.WriteLine();
    }

    private static string Flatten(string message)
    {
        var flat = string.Join(' ', message
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim()));

        return flat.Length <= 2000 ? flat : flat[..2000] + " ...";
    }

    private static string Shape(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return "(not captured)";
        }

        var flat = string.Join(' ', sql
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim()));

        return flat.Length <= 200 ? flat : flat[..200] + " ...";
    }
}
