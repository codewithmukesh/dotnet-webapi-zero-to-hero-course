# HTTP Resilience Without Polly in .NET 10

Sample code for the article [How to Implement HTTP Resilience Without Polly in .NET 10](https://codewithmukesh.com/blog/http-resilience-without-polly-dotnet/).

It compares three ways to add retries, timeouts and a circuit breaker to `HttpClient`:

| Option | Package | Polly in the dependency graph? |
|---|---|---|
| Microsoft standard handler | `Microsoft.Extensions.Http.Resilience` 10.11.0 | Yes, transitively (Polly.Core 8.4.2) |
| Kevlar | `Kevlar.Extensions.Http` 1.4.1 (MIT) | No |
| Hand-rolled | none (`HttpResilience.Handlers`) | No |

## Projects

- `HttpResilience.Handlers` - the hand-rolled `RetryHandler`, `CircuitBreakerHandler` and `AttemptTimeoutHandler`, plus `AddHandRolledResilience()`.
- `HttpResilience.FlakyApi` - a fake downstream API that fails on purpose (503s, 429 with Retry-After, hangs, a dead service).
- `HttpResilience.Harness` - runs the same failure scenarios through all three options and prints a results table.
- `HttpResilience.Api` - a minimal API that registers all three options (Scalar at `/scalar/v1`).

## Run the comparison

```bash
dotnet run --project HttpResilience.Harness -c Release
```

## Run the API

```bash
dotnet run --project HttpResilience.FlakyApi --urls http://localhost:5100
dotnet run --project HttpResilience.Api
```

Then call `GET /products/handrolled?failures=2` (or `microsoft` / `kevlar`).

## Check whether your app references Polly directly

```bash
dotnet list package --include-transitive
```

Requires the .NET 10 SDK.
