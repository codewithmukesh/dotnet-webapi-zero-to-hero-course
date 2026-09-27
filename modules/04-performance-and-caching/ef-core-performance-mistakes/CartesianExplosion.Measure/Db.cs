using System.Collections.Concurrent;
using CartesianExplosion.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CartesianExplosion.Measure;

/// <summary>
/// One DbContextOptions per process, reused for every context. Building new options per run would make
/// EF Core create a new internal service provider each time (and it throws after twenty).
/// </summary>
/// <remarks>
/// EF Core logs MultipleCollectionIncludeWarning when it COMPILES a query, and compiled queries are cached
/// per process. So the warning shows up once per query shape, not on every request.
/// <paramref name="isolatedQueryCache"/> gives this instance its own cache so it sees the warning again
/// (tests only - it rebuilds EF's internals for every context, which would skew timings).
/// </remarks>
public sealed class Db
{
    public Db(string connectionString, bool isolatedQueryCache = false)
    {
        ConnectionString = connectionString;
        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(Capture)
            .LogTo(Warnings.Enqueue, [RelationalEventId.MultipleCollectionIncludeWarning], LogLevel.Warning)
            .EnableServiceProviderCaching(!isolatedQueryCache)
            .Options;
    }

    public string ConnectionString { get; }

    public SqlCapture Capture { get; } = new();

    public ConcurrentQueue<string> Warnings { get; } = new();

    public DbContextOptions<AppDbContext> Options { get; }

    public AppDbContext NewContext() => new(Options);
}
