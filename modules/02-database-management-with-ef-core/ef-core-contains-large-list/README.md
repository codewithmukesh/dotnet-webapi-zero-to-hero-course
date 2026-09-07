# Filtering EF Core Queries by a Large List

Companion code for [Filtering EF Core Queries by a Large List: The 2,100 Parameter Wall](https://codewithmukesh.com/blog/ef-core-contains-large-list/).

Everything here is runnable. The numbers in the article come from these projects, not from a spreadsheet.

## What this proves

SQL Server caps a query at 2,100 parameters. Through EF Core the usable ceiling is **2,098**, because SqlClient sends the query via `sp_executesql`, which spends two parameters itself.

What EF Core 10 does when you cross that ceiling is the interesting part, and it is not what most articles on this topic say:

| List size | Parameters sent | Translation |
|---|---|---|
| 2,098 | 2,098 | `IN (@ids1, @ids2, ... @ids2098)` |
| 2,099 | 1 | `IN (SELECT [Value] FROM OPENJSON(@ids) ...)` |

`Contains` does not throw. EF switches to a single JSON parameter on its own and keeps working, verified here up to 100,000 ids.

## Projects

| Project | What it does |
|---|---|
| `Contains.Shared` | `Product` entity, the composite-key `InventoryItem` entity, `AppDbContext` with a configurable `ParameterTranslationMode`, a `DbCommandInterceptor` that captures the real SQL, and both seeders |
| `Contains.Probe` | Captures what EF actually sends across list sizes, translation modes, and the boundary. Writes `probe-output.md` |
| `Contains.Benchmarks` | BenchmarkDotNet matrices: seven approaches across eight list sizes for scalars, six approaches across five list sizes for composite keys |
| `Contains.Api` | Minimal API showing which approach fits which endpoint |

## Requirements

- .NET 10 SDK
- SQL Server. The connection string in `Contains.Shared/DbSetup.cs` points at LocalDB (`(localdb)\MSSQLLocalDB`); point it anywhere you like.

Measured on EF Core **10.0.11** / .NET **10.0.11**. The parameter ceiling moved in 10.0.2 ([#37336](https://github.com/dotnet/efcore/issues/37336)), so run 10.0.2 or later if you want to reproduce these results.

## Running it

The first run creates the database and bulk-copies 1,000,000 rows, which takes a few seconds. It is idempotent, so later runs skip it.

```bash
# What EF actually sends - writes probe-output.md
dotnet run --project Contains.Probe -c Release

# Check every approach agrees before spending time on the full matrix
dotnet run --project Contains.Benchmarks -c Release -- --verify

# The full benchmark matrix
dotnet run --project Contains.Benchmarks -c Release

# The API
dotnet run --project Contains.Api
```

### Composite keys

A second set of modes covers the case where the filter list is `(TenantId, ProductId)` pairs against the one-million-row `Inventory` table.

```bash
# Every composite-key approach at one list size, each in its own process
dotnet run --project Contains.Probe -c Release -- --composite 400

# One approach on its own (this is what --composite spawns)
dotnet run --project Contains.Probe -c Release -- --composite-one orchain-param 400

# Bisect the largest list an approach survives
dotnet run --project Contains.Probe -c Release -- --composite-limit orchain-param 1 4000

# The composite-key benchmark matrix
dotnet run --project Contains.Benchmarks -c Release -- --composite
```

Approach names for `--composite-one` and `--composite-limit`: `valuetuple`, `anonymous`, `any`, `orchain-param`, `orchain-const`, `orchain-balanced`, `stringkey`, `efe-bulk`, `efplus`, `temptable`.

Each approach runs in its own child process on purpose. An OR chain built from enough pairs overflows the stack while EF walks the expression tree, and a stack overflow cannot be caught: it takes the process down with `STATUS_STACK_OVERFLOW` (`0xC00000FD`). Running them in one process would lose every later result.

## The composite-key approaches

| Approach | Result |
|---|---|
| `keys.Contains(new ValueTuple<int, int>(...))` | Does not translate, at any size |
| `keys.Contains(new { ... })` | Does not translate, at any size |
| `keys.Any(k => k.A == i.A && k.B == i.B)` | Does not translate, at any size |
| OR chain, left-deep (what a `foreach` builds) | Kills the process at 490 pairs. 489 works |
| OR chain, balanced | Survives to 1,049 pairs, then throws the real 2,100 parameter error |
| String key `Contains` | Translates, then scans: both columns are `CAST` before comparison |
| Temp table + join on both columns | No parameters, no ceiling |
| `WhereBulkContains` | No parameters, no ceiling, stays in `IQueryable` |
| `WhereContains` (free) | Resolves to an inlined OR chain, so it inherits the stack ceiling |

The 489/490 edge is a stack depth limit, so it moves with stack size, build configuration and runtime version. Reproduce it rather than quoting it.

### Reading the composite benchmark output

`CompositeKeyBenchmarks` refuses to build an OR chain past `OrChainCeiling` (489) and returns `-1` instead, because a stack overflow would take the BenchmarkDotNet runner down with it rather than failing one case.

That means **the sub-microsecond rows in the report are not measurements.** `OrChain_Parameters`, `OrChain_Constants` and `WhereContains_EFPlus` show times like `162.5 ns` at 1,000 pairs and above. That is the guard returning early, not a fast query. Read those cells as "cannot run at this size".

Similarly, the `StringKey_Contains` rows sitting at almost exactly `30 s` are the default SqlClient command timeout expiring, not a completed query. It only returns real numbers at 5,000 and 100,000 pairs, where the key list itself crosses 2,098 values and picks up the `OPENJSON` fallback.

Run `--composite-one <approach> <size>` if you want to see which of those a given cell is.

### A trap worth knowing

EF caches compiled queries per model, so probing several **global** `UseParameterizedCollectionMode` settings inside one process hands you the first mode's SQL for every later run. `Contains.Probe` measures each global mode in its own child process for exactly this reason. Per-query `EF.Parameter` and `EF.Constant` do not have the problem, because they change the expression tree and so get their own cache entry.

## The seven approaches

| # | Approach | Notes |
|---|---|---|
| 1 | `Contains` (EF Core 10 default) | Multiple scalar parameters, auto-fallback to `OPENJSON` past 2,098 |
| 2 | `EF.Parameter(ids).Contains(...)` | Single JSON array parameter - the EF Core 8/9 default |
| 3 | `EF.Constant(ids).Contains(...)` | Values inlined into the SQL text |
| 4 | Manual chunking | Batches of 2,000, unioned client-side |
| 5 | Temp table + `INNER JOIN` | `SqlBulkCopy` into `#Ids`, then join |
| 6 | `WhereBulkContains` | Entity Framework Extensions (commercial) |
| 7 | `WhereContains` | Entity Framework Plus (free) |

Approaches 3 and 7 return `-1` at large list sizes, where SQL Server rejects the query with *"The query processor ran out of internal resources and could not produce a query plan."* That is a real result, not a harness failure, and it is a different failure from the 2,100 parameter error.

## Licensing

Entity Framework Extensions (`Z.EntityFramework.Extensions.EFCore`) is a commercial library and `WhereBulkContains` is one of its paid methods. Entity Framework Plus (`Z.EntityFramework.Plus.EFCore`) is free, but its `WhereBulk` resolution routes into Entity Framework Extensions and needs that licence. Check current terms before you depend on either in production.
