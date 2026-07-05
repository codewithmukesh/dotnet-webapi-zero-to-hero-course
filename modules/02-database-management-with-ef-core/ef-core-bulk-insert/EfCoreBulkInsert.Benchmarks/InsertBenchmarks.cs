using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EFCore.BulkExtensions;
using EfCoreBulkInsert.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace EfCoreBulkInsert.Benchmarks;

// RunStrategy.Monitoring runs one invocation per iteration, which is the correct
// mode for benchmarks that mutate a database (each run inserts real rows).
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, iterationCount: 5)]
public class InsertBenchmarks
{
    private const string ConnectionString =
        "Host=localhost;Port=5433;Database=bulk_insert_db;Username=postgres;Password=postgres";

    private const int ChunkSize = 5_000;

    [Params(1_000, 10_000)]
    public int RowCount { get; set; }

    private List<Product> _products = null!;

    private static DbContextOptions<AppDbContext> BuildOptions() =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableThreadSafetyChecks(false)
            .Options;

    private AppDbContext NewContext() => new(BuildOptions());

    [GlobalSetup]
    public void GlobalSetup()
    {
        using var context = NewContext();
        context.Database.EnsureCreated();

        _products = Enumerable.Range(0, RowCount)
            .Select(i => new Product
            {
                Name = $"Product {i}",
                Sku = $"SKU-{i:D8}",
                Price = 9.99m + i % 500,
                Category = $"Category {i % 20}",
                Description = "Imported from the nightly product feed.",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            })
            .ToList();
    }

    [IterationSetup]
    public void ResetTable()
    {
        using var context = NewContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Products\" RESTART IDENTITY;");
        // Give every entity a fresh, untracked identity so re-inserting is clean.
        foreach (var product in _products)
        {
            product.Id = 0;
        }
    }

    // The mistake almost everyone starts with: one round trip per row.
    [Benchmark]
    public void AddThenSaveEachRow()
    {
        using var context = NewContext();
        foreach (var product in _products)
        {
            context.Products.Add(product);
            context.SaveChanges();
        }
    }

    // The sensible native default: one AddRange, one SaveChanges (batched).
    [Benchmark(Baseline = true)]
    public void AddRangeThenSaveOnce()
    {
        using var context = NewContext();
        context.Products.AddRange(_products);
        context.SaveChanges();
    }

    // Same as above, with automatic change detection turned off for the hot path.
    [Benchmark]
    public void AddRangeAutoDetectOff()
    {
        using var context = NewContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        context.Products.AddRange(_products);
        context.SaveChanges();
    }

    // Chunked inserts that clear the tracker between batches to keep memory flat.
    [Benchmark]
    public void ChunkedAddRangeClear()
    {
        using var context = NewContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        foreach (var chunk in _products.Chunk(ChunkSize))
        {
            context.Products.AddRange(chunk);
            context.SaveChanges();
            context.ChangeTracker.Clear();
        }
    }

    // EFCore.BulkExtensions: uses PostgreSQL binary COPY under the hood.
    [Benchmark]
    public void BulkExtensionsBulkInsert()
    {
        using var context = NewContext();
        context.BulkInsert(_products);
    }

    // Raw Npgsql binary COPY: the fastest path for PostgreSQL, no EF at all.
    [Benchmark]
    public void NpgsqlBinaryCopy()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var writer = connection.BeginBinaryImport(
            "COPY \"Products\" (\"Name\", \"Sku\", \"Price\", \"Category\", \"Description\", \"IsActive\", \"CreatedAtUtc\") FROM STDIN (FORMAT BINARY)");

        foreach (var product in _products)
        {
            writer.StartRow();
            writer.Write(product.Name, NpgsqlDbType.Varchar);
            writer.Write(product.Sku, NpgsqlDbType.Varchar);
            writer.Write(product.Price, NpgsqlDbType.Numeric);
            writer.Write(product.Category, NpgsqlDbType.Varchar);
            if (product.Description is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.Write(product.Description, NpgsqlDbType.Varchar);
            }
            writer.Write(product.IsActive, NpgsqlDbType.Boolean);
            writer.Write(product.CreatedAtUtc, NpgsqlDbType.TimestampTz);
        }

        writer.Complete(); // Mandatory: skipping this rolls the whole import back.
    }
}
