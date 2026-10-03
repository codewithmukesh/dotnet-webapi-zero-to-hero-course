# Best Libraries for ASP.NET Core - Sample Project

A small .NET 10 shop API that uses the 12 NuGet packages from the article [Best Libraries for ASP.NET Core in 2026](https://codewithmukesh.com/blog/best-libraries-for-aspnet-core/).

| # | Job | Package |
|---|-----|---------|
| 1 | Logging | Serilog.AspNetCore |
| 2 | Validation | FluentValidation |
| 3 | Object mapping | Riok.Mapperly |
| 4 | Data access | EF Core 10 (PostgreSQL) and Dapper |
| 5 | API documentation | Scalar.AspNetCore |
| 6 | Caching | Microsoft.Extensions.Caching.Hybrid |
| 7 | Scheduled jobs | Quartz.Extensions.Hosting |
| 8 | Command and query handlers | Mediator |
| 9 | Traces and metrics | OpenTelemetry |
| 10 | Unit tests | xunit.v3 and NSubstitute |
| 11 | Integration tests | Testcontainers.PostgreSql |
| 12 | Running everything locally | Aspire |

## Prerequisites

- .NET 10 SDK
- Docker Desktop

## Run the API

```bash
dotnet run --project src/AppHost
```

Aspire starts PostgreSQL, Redis and the API, and prints the link to its dashboard in the console. The API runs at `http://localhost:5080`, and the Scalar page is at `http://localhost:5080/scalar`.

## Run the tests

```bash
dotnet test
```

The integration tests start a PostgreSQL container with Testcontainers, so Docker has to be running.
