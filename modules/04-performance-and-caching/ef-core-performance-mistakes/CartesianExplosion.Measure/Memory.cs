using System.Diagnostics;

namespace CartesianExplosion.Measure;

public static class Memory
{
    /// <summary>
    /// `memory --mode single --documents true`: runs ONE query in a fresh process and prints peak memory.
    /// The database must already be seeded. Run each mode in its own process so they can't pollute each other.
    /// </summary>
    public static async Task<int> RunAsync(Options options, CancellationToken ct)
    {
        var mode = options.Get("mode", "single");
        var documents = options.GetBool("documents", true);
        var db = new Db(options.ConnectionString);

        var before = GC.GetTotalAllocatedBytes(precise: true);
        var departments = await Bench.RunOnceAsync(db, mode, documents, ct);
        var allocated = GC.GetTotalAllocatedBytes(precise: true) - before;

        using var process = Process.GetCurrentProcess();
        process.Refresh();
        Console.WriteLine(Json.Line(new
        {
            mode,
            documents,
            departments,
            peakWorkingSetMb = Math.Round(process.PeakWorkingSet64 / 1048576d, 1),
            allocatedMb = Math.Round(allocated / 1048576d, 1),
            gcHeapMb = Math.Round(GC.GetGCMemoryInfo().HeapSizeBytes / 1048576d, 1),
        }));
        return 0;
    }
}
