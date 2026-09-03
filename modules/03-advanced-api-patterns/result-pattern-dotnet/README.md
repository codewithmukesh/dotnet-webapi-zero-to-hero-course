# Result Pattern in .NET 10

Companion code for [Result Pattern in C# and .NET 10 - When to Use, When to Skip](https://codewithmukesh.com/blog/result-pattern-dotnet/).

## Projects

| Project | What it is |
|---|---|
| `ResultPattern.Api` | Minimal API on .NET 10 showing `Result` / `Result<T>`, a typed `Error` record, an error catalogue, async service methods over EF Core 10 + SQLite, and a single mapping point from domain errors to RFC 9457 ProblemDetails. |
| `ResultPattern.Benchmarks` | BenchmarkDotNet project measuring a `Result` return against a thrown exception at call-stack depth 1 and depth 10. |
| `ResultPattern.Tests` | xUnit v3 tests asserting failed results by `Error.Code`, over an in-memory SQLite database. |

## Run the API

```bash
dotnet run --project ResultPattern.Api
```

Then open `http://localhost:5000/scalar/v1`. The database is a local SQLite file
(`orders.db`) created on startup with `EnsureCreatedAsync`, so there is nothing to install.

### Endpoints

| Endpoint | What it shows |
|---|---|
| `GET /orders/{id}` | A `NotFound` error mapped to a 404 ProblemDetails |
| `POST /orders` | Fail-fast validation: the first `Error` becomes a 400 |
| `POST /orders/bulk-validated` | Every validation failure in one 400, as an RFC 9457 `errors` dictionary |
| `POST /orders/{id}/cancel` | The non-generic `Result`, plus a `Conflict` error mapped to a 409 |

## Run the tests

```bash
dotnet run --project ResultPattern.Tests
```

xUnit v3 test projects are executables, so the suite runs with `dotnet run`, not `dotnet test`.

## Run the benchmarks

```bash
dotnet run --project ResultPattern.Benchmarks -c Release
```

Benchmark output from the run used in the article lives in
`ResultPattern.Benchmarks/BenchmarkDotNet.Artifacts/results/`.
