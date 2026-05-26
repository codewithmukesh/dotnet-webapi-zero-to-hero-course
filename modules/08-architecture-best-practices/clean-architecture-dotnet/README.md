# Movie Management - Clean Architecture in .NET 10

Companion source code for the article **[Implementing Clean Architecture in .NET - Step-by-Step Guide](https://codewithmukesh.com/blog/clean-architecture-dotnet/)** on codewithmukesh.com.

A movie management Web API built with Clean Architecture, .NET 10, EF Core 10, PostgreSQL, and .NET Aspire for local orchestration. It uses `DbContext` directly (no repository pattern) and a small Domain layer with light DDD.

## Stack

- .NET 10 / C# 14
- EF Core 10 + PostgreSQL (Npgsql)
- Minimal APIs + Scalar for API docs
- .NET Aspire 13 (AppHost + ServiceDefaults)
- Central Package Management (`Directory.Packages.props`)

## Projects

```
src/
  MovieManagement.Domain          # Entities, value objects, business rules. No dependencies.
  MovieManagement.Application      # Use cases, DTOs, IApplicationDbContext. Depends on Domain.
  MovieManagement.Infrastructure   # EF Core DbContext, configurations. Depends on Application.
  MovieManagement.Api              # Minimal API endpoints, composition root. Depends on all.
aspire/
  MovieManagement.AppHost          # Orchestrates the API + PostgreSQL container.
  MovieManagement.ServiceDefaults  # Shared telemetry, health checks, resilience.
```

## Run it

You need the .NET 10 SDK and Docker Desktop (Aspire starts PostgreSQL in a container).

```bash
dotnet run --project aspire/MovieManagement.AppHost
```

The Aspire dashboard opens with the API and PostgreSQL wired together. Open the API resource, then browse to `/scalar/v1` to try the endpoints.

## Migrations

```bash
dotnet ef migrations add InitialCreate --project src/MovieManagement.Infrastructure --startup-project src/MovieManagement.Api
```
