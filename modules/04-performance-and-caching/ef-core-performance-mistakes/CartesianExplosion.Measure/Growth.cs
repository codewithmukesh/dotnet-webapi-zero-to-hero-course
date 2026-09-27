using System.Diagnostics;
using CartesianExplosion.Api.Data;

namespace CartesianExplosion.Measure;

public static class Growth
{
    /// <summary>`growth --sizes 50x5x8x0,50x10x15x0,... --runs 7`: query time as the collections grow.</summary>
    public static async Task<int> RunAsync(Options options, CancellationToken ct)
    {
        var sizes = options.Get("sizes", "50x5x8x0,50x10x15x0,50x20x30x0,50x40x60x0,50x80x120x0")
            .Split(',')
            .Select(SeedSize.Parse)
            .ToArray();
        var runs = options.GetInt("runs", 7);
        var db = new Db(options.ConnectionString);
        var points = new List<object>();

        foreach (var size in sizes)
        {
            await using (var context = db.NewContext())
            {
                await Seeder.ResetAsync(context, size, ct);
            }

            var single = await TimeAsync(db, "single", runs, ct);
            var split = await TimeAsync(db, "split", runs, ct);
            points.Add(new
            {
                size,
                singleRows = size.SingleQueryRows(false),
                splitRows = size.RecordCount(false),
                singleMedianMs = single,
                splitMedianMs = split,
            });
            Console.WriteLine($"{size}: single {size.SingleQueryRows(false):N0} rows {single:F0} ms | split {size.RecordCount(false):N0} rows {split:F0} ms");
        }

        await Json.WriteAsync("results/growth.json", new { runs, points }, ct);
        return 0;
    }

    private static async Task<double> TimeAsync(Db db, string mode, int runs, CancellationToken ct)
    {
        await Bench.RunOnceAsync(db, mode, false, ct);
        var times = new List<double>(runs);
        for (var i = 0; i < runs; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            await Bench.RunOnceAsync(db, mode, false, ct);
            times.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        return Stats.Median(times);
    }
}
