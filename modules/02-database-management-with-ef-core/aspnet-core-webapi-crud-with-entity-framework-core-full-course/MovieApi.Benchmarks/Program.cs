using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MovieApi.Api.Models;
using MovieApi.Api.Persistence;

const int iterations = 50;
const int warmup = 5;

int[] sizes = [1_000, 10_000, 100_000];

Console.WriteLine();
Console.WriteLine("AsNoTracking vs Tracking - Movies List Query");
Console.WriteLine($".NET 10, EF Core 10, InMemory provider, {iterations} iterations after {warmup} warmup runs");
Console.WriteLine();
Console.WriteLine($"{"Movies",10} | {"Method",15} | {"Mean (ms)",10} | {"Allocations (KB)",18} | {"Speedup",8}");
Console.WriteLine(new string('-', 80));

foreach (var size in sizes)
{
    var dbName = $"MoviesDb_{size}_{Guid.NewGuid()}";
    var options = new DbContextOptionsBuilder<MovieDbContext>()
        .UseInMemoryDatabase(dbName)
        .Options;

    using (var seedContext = new MovieDbContext(options))
    {
        var movies = Enumerable.Range(0, size)
            .Select(i => Movie.Create(
                title: $"Movie {i}",
                genre: i % 3 == 0 ? "Action" : i % 3 == 1 ? "Drama" : "Comedy",
                releaseDate: new DateTimeOffset(new DateTime(2000, 1, 1).AddDays(i % 9000), TimeSpan.Zero),
                rating: (i % 10) + 0.5))
            .ToList();
        seedContext.Movies.AddRange(movies);
        seedContext.SaveChanges();
    }

    var trackingResult = Measure(options, tracking: true, iterations, warmup);
    var noTrackingResult = Measure(options, tracking: false, iterations, warmup);

    var speedup = trackingResult.MeanMs / noTrackingResult.MeanMs;

    Console.WriteLine($"{size,10:N0} | {"Tracking",15} | {trackingResult.MeanMs,10:F2} | {trackingResult.AllocatedKb,18:F1} | {1.00,8:F2}x");
    Console.WriteLine($"{size,10:N0} | {"AsNoTracking",15} | {noTrackingResult.MeanMs,10:F2} | {noTrackingResult.AllocatedKb,18:F1} | {speedup,8:F2}x");
    Console.WriteLine();
}

static (double MeanMs, double AllocatedKb) Measure(DbContextOptions<MovieDbContext> options, bool tracking, int iterations, int warmup)
{
    for (int i = 0; i < warmup; i++)
    {
        using var ctx = new MovieDbContext(options);
        _ = tracking ? ctx.Movies.ToList() : ctx.Movies.AsNoTracking().ToList();
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var allocBefore = GC.GetAllocatedBytesForCurrentThread();
    var sw = Stopwatch.StartNew();

    for (int i = 0; i < iterations; i++)
    {
        using var ctx = new MovieDbContext(options);
        _ = tracking ? ctx.Movies.ToList() : ctx.Movies.AsNoTracking().ToList();
    }

    sw.Stop();
    var allocAfter = GC.GetAllocatedBytesForCurrentThread();

    var meanMs = sw.Elapsed.TotalMilliseconds / iterations;
    var allocatedKb = (allocAfter - allocBefore) / 1024.0 / iterations;

    return (meanMs, allocatedKb);
}
