using System.Diagnostics;
using BenchmarkDotNet.Running;
using EfCoreContainsLargeList.Benchmarks;

// "--verify" runs every approach once per size and checks they agree, before committing
// to the full BenchmarkDotNet matrix. Catches licensing and API surprises in seconds.
if (args is ["--verify", ..])
{
    var bench = new ContainsBenchmarks();

    (string Name, Func<ContainsBenchmarks, int> Run)[] approaches =
    [
        ("Contains_Default", b => b.Contains_Default()),
        ("Contains_EfParameter", b => b.Contains_EfParameter()),
        ("Contains_EfConstant", b => b.Contains_EfConstant()),
        ("Contains_Chunked", b => b.Contains_Chunked()),
        ("TempTable_Manual", b => b.TempTable_Manual()),
        ("WhereBulkContains_EFE", b => b.WhereBulkContains_EFE()),
        ("WhereContains_EFPlus", b => b.WhereContains_EFPlus())
    ];

    foreach (var size in (int[])[10, 2_098, 2_099, 5_000, 100_000])
    {
        bench.ListSize = size;
        bench.Setup();
        Console.WriteLine($"-- list size {size:N0} (expecting {size:N0} matches) --");

        foreach (var (name, run) in approaches)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var count = run(bench);
                sw.Stop();
                var verdict = count == size ? "ok" : $"MISMATCH got {count:N0}";
                Console.WriteLine($"   {name,-24} {sw.ElapsedMilliseconds,7:N0} ms   {verdict}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   {name,-24} {"-",7}      FAILED {ex.GetType().Name}: {ex.Message.Split('\n')[0].Trim()}");
            }
        }

        Console.WriteLine();
    }

    return;
}

if (args is ["--composite", ..])
{
    BenchmarkRunner.Run<CompositeKeyBenchmarks>();
    return;
}

BenchmarkRunner.Run<ContainsBenchmarks>();
