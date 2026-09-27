using System.Diagnostics;
using System.Globalization;
using System.Text;
using CartesianExplosion.Api.Data;
using CartesianExplosion.Api.Queries;
using Microsoft.EntityFrameworkCore;

namespace CartesianExplosion.Measure;

public sealed record BenchResult(
    string Mode, bool Documents, IReadOnlyList<ResultSize.Statement> Statements, double MedianMs, double MedianAllocatedBytes)
{
    public long Rows => Statements.Sum(s => s.Rows);

    public long Bytes => Statements.Sum(s => s.Bytes);
}

public static class Bench
{
    /// <summary>`bench --size Article --runs 15`: seeds, then measures single vs split, with and without documents.</summary>
    public static async Task<int> RunAsync(Options options, CancellationToken ct)
    {
        var sizeName = options.Get("size", "Article");
        var size = SeedSize.Parse(sizeName);
        var runs = options.GetInt("runs", 15);
        var db = new Db(options.ConnectionString);

        await using (var context = db.NewContext())
        {
            await Seeder.ResetAsync(context, size, ct);
        }

        var results = new List<BenchResult>();
        foreach (var documents in new[] { false, true })
        {
            foreach (var mode in new[] { "single", "split" })
            {
                results.Add(await MeasureAsync(db, mode, documents, runs, ct));
            }
        }

        Directory.CreateDirectory("results/sql");
        foreach (var r in results)
        {
            var name = $"results/sql/{r.Mode}{(r.Documents ? "-documents" : "")}.sql";
            await File.WriteAllTextAsync(name, string.Join(";\n\n", r.Statements.Select(s => s.Sql)) + ";\n", ct);
        }

        await File.WriteAllLinesAsync("results/warning.txt", db.Warnings.Distinct(), ct);
        await Json.WriteAsync($"results/bench-{sizeName.ToLowerInvariant()}.json", new { size, runs, results }, ct);
        Console.WriteLine(Table(size, results));
        return 0;
    }

    public static async Task<BenchResult> MeasureAsync(Db db, string mode, bool documents, int runs, CancellationToken ct)
    {
        // First run compiles the query, logs any warning and captures the SQL.
        db.Capture.Clear();
        await RunOnceAsync(db, mode, documents, ct);
        var sql = db.Capture.Commands.ToArray();

        var statements = new List<ResultSize.Statement>();
        foreach (var s in sql)
        {
            statements.Add(await ResultSize.MeasureAsync(db.ConnectionString, s, ct));
        }

        for (var i = 0; i < 2; i++)
        {
            await RunOnceAsync(db, mode, documents, ct);
        }

        var times = new List<double>(runs);
        var allocations = new List<double>(runs);
        for (var i = 0; i < runs; i++)
        {
            var before = GC.GetTotalAllocatedBytes(precise: true);
            var stopwatch = Stopwatch.StartNew();
            await RunOnceAsync(db, mode, documents, ct);
            stopwatch.Stop();
            allocations.Add(GC.GetTotalAllocatedBytes(precise: true) - before);
            times.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        return new BenchResult(mode, documents, statements, Stats.Median(times), Stats.Median(allocations));
    }

    public static async Task<int> RunOnceAsync(Db db, string mode, bool documents, CancellationToken ct)
    {
        await using var context = db.NewContext();
        var query = mode == "split"
            ? DepartmentQueries.Split(context, documents)
            : DepartmentQueries.Single(context, documents);
        var departments = await query.ToListAsync(ct);
        return departments.Count;
    }

    private static string Table(SeedSize size, IEnumerable<BenchResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Seed: {size}");
        sb.AppendLine("| mode | documents | statements | rows | result size (MB) | median ms | allocated (MB) |");
        sb.AppendLine("|---|---|---|---|---|---|---|");
        foreach (var r in results)
        {
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"| {r.Mode} | {r.Documents} | {r.Statements.Count} | {r.Rows:N0} | {r.Bytes / 1048576d:F1} | {r.MedianMs:F0} | {r.MedianAllocatedBytes / 1048576d:F1} |");
        }

        return sb.ToString();
    }
}
