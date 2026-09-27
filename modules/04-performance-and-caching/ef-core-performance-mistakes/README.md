# 10 EF Core Performance Mistakes - Cartesian explosion demo

Source code for Mistake 5 of the lesson **[10 EF Core Performance Mistakes (and How to Fix Them) in .NET 10](https://codewithmukesh.com/blog/ef-core-performance-mistakes/)** from the free .NET Web API Zero to Hero course.

When you `Include` two collections side by side, EF Core's default single query LEFT JOINs both of them. The database can only send back one flat table, so every project is paired with every employee and the rows multiply. This demo measures what that costs, and what `AsSplitQuery()` changes.

```csharp
// the query most of us write
context.Departments
    .Include(d => d.Projects)
    .Include(d => d.Employees)
    .ToListAsync(ct);

// the fix: one extra query per included collection
context.Departments
    .Include(d => d.Projects)
    .Include(d => d.Employees)
    .AsSplitQuery()
    .ToListAsync(ct);
```

.NET 10, EF Core 10.0.12, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3, PostgreSQL 17, `.slnx`.

## Projects

| Project | What it is |
|---|---|
| `CartesianExplosion.Api` | Minimal API: `GET /departments/single`, `GET /departments/split` (add `?documents=true` for a third collection) and `GET /health` (a DbContext health check that uses the same connection pool) |
| `CartesianExplosion.Measure` | Console tool that produced every number below: `bench`, `growth`, `memory`, `load` |
| `CartesianExplosion.Tests` | xUnit v3 + Testcontainers tests that pin the row math and EF Core's warning |

## Run it

```bash
docker compose up -d
dotnet run --project CartesianExplosion.Api -- --Seed:Size Tiny
```

`Tiny` seeds one department, Engineering, with the projects Apollo, Borealis and Comet and the employees Asha, Ben, Chen and Dev. Then:

```bash
curl http://localhost:5080/departments/single
curl http://localhost:5080/departments/split
```

Both return the same department. The first one also makes EF Core log this warning:

```text
warn: Microsoft.EntityFrameworkCore.Query[20504]
      Compiling a query which loads related collections for more than one collection navigation, either via 'Include' or through projection, but no 'QuerySplittingBehavior' has been configured. By default, Entity Framework will use 'QuerySplittingBehavior.SingleQuery', which can potentially result in slow query performance.
```

EF Core logs it when it **compiles** the query, so you see it once per query shape, not on every request. It's easy to miss.

Other seed sizes: `--Seed:Size Article` (50 departments × 20 projects × 30 employees × 10 documents) or any `DxPxExD` such as `50x40x60x0`.

## Tests

Needs Docker running.

```bash
dotnet run --project CartesianExplosion.Tests
```

## Measure it yourself

Run from this folder with Postgres up. Every command writes to `results/`.

```bash
dotnet run -c Release --project CartesianExplosion.Measure -- bench --size Article --runs 15
dotnet run -c Release --project CartesianExplosion.Measure -- growth --runs 7
dotnet run -c Release --project CartesianExplosion.Measure -- memory --mode single --documents true
dotnet run -c Release --project CartesianExplosion.Measure -- load --mode single --documents true --concurrency 200 --seconds 30
```

`memory` expects an already seeded database (run `bench` first). `load` expects the API running on `http://localhost:5080`. Restart the API, and end any leftover Postgres backends, between load runs, or the previous run's queries will still be holding connections.

## Results on my machine

Machine, versions and caveats are in [results/ENVIRONMENT.md](results/ENVIRONMENT.md). The most important caveat: the database runs locally, so round trips are nearly free, which flatters split queries.

### One query, article-size data ([bench-article.json](results/bench-article.json))

50 departments, 20 projects and 30 employees each. The 2,550 records you actually need are 50 departments + 1,000 projects + 1,500 employees.

| | statements | rows sent | result size | median time | allocated per request |
|---|---|---|---|---|---|
| Single query | 1 | 30,000 | 9.7 MB | 96 ms | 21.1 MB |
| `AsSplitQuery()` | 3 | 2,550 | 0.3 MB | 6 ms | 3.3 MB |
| Single, + 10 documents each | 1 | 300,000 | 112.2 MB | 1,326 ms | 291.6 MB |
| Split, + 10 documents each | 4 | 3,050 | 0.3 MB | 6 ms | 3.8 MB |

The exact SQL for each is in [results/sql/](results/sql/). "Result size" is Postgres's `pg_column_size` summed over the returned rows, which is close to, but not exactly, the bytes on the wire.

### As the data grows ([growth.json](results/growth.json))

50 departments; projects and employees per department double at each step.

| projects × employees | single: rows | single: median | split: rows | split: median |
|---|---|---|---|---|
| 5 × 8 | 2,000 | 14 ms | 700 | 5 ms |
| 10 × 15 | 7,500 | 44 ms | 1,300 | 12 ms |
| 20 × 30 | 30,000 | 144 ms | 2,550 | 19 ms |
| 40 × 60 | 120,000 | 306 ms | 5,050 | 8 ms |
| 80 × 120 | 480,000 | 1,132 ms | 10,050 | 20 ms |

When both collections double, the single query's rows go up four times. The split query's rows only double.

### Memory ([oom.md](results/oom.md))

With documents included (300,000 rows), one request with the single query allocated about **309 MB**, against about **10.6 MB** for the split query. Peak memory barely moved (103.5 MB vs 82.4 MB), because EF Core reads a single query as a stream and the garbage collector reclaims the row data as it goes. The cost is garbage-collection work, not one big memory spike.

### Under load ([load-*.json](results/), [load-errors.md](results/load-errors.md))

The `/departments/*?documents=true` endpoint for 30 seconds, while a probe calls `/health` every 250 ms. Npgsql's defaults: 100 pooled connections, 15-second wait for a free one.

| concurrent users | single: completed, median | single: 500s | split: completed, median | `/health` during single | `/health` during split |
|---|---|---|---|---|---|
| 100 | 110, 24.3 s | 0 | 15,681, 174 ms | all OK, but up to 13.3 s | all OK, p95 217 ms |
| 200 | 170, 24.8 s | 53 | 15,414, 358 ms | **503 after 24 s** | all OK |
| 400 | 336, 24.2 s | 214 | 15,657, 681 ms | **503 after 23 s** | all OK |

Every 500 was Npgsql running out of pooled connections:

```text
The connection pool has been exhausted, either raise 'Max Pool Size' (currently 100) or 'Timeout' (currently 15 seconds) in your connection string.
```

The health check uses the same pool, so once the slow query holds every connection, even `/health` has to wait in line and starts failing.

The same test with the 30,000-row query (no documents, `load-single-c*.json` / `load-split-c*.json`) never exhausted the pool, but the gap is just as wide:

| concurrent users | single: completed, median | split: completed, median | slowest `/health` during single | slowest `/health` during split |
|---|---|---|---|---|
| 100 | 2,043, 1.4 s | 14,816, 168 ms | 1.4 s | 0.4 s |
| 200 | 2,192, 2.4 s | 17,554, 314 ms | 2.7 s | 0.5 s |
| 400 | 2,311, 4.7 s | 16,424, 635 ms | 4.4 s | 1.2 s |

At 200 users the split query served 8x as many requests.
