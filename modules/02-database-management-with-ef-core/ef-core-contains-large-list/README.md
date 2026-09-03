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
| `Contains.Shared` | `Product` entity, `AppDbContext` with a configurable `ParameterTranslationMode`, a `DbCommandInterceptor` that captures the real SQL, and the seeder |
| `Contains.Probe` | Captures what EF actually sends across list sizes, translation modes, and the boundary. Writes `probe-output.md` |
| `Contains.Benchmarks` | BenchmarkDotNet matrix: seven approaches across eight list sizes |
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
