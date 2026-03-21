# Multiple DbContext in EF Core 10

When and how to use multiple DbContext in EF Core 10 - multi-database setup, schema separation, read replicas, cross-context transactions, and modular monolith patterns.

## Resources

- **Article**: [Multiple DbContext in EF Core 10 - Scenarios, Setup & Migrations](https://codewithmukesh.com/blog/multiple-dbcontext-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Set up multiple DbContexts with separate DI registrations
- Manage migrations independently with `--context` and `--output-dir`
- Configure schema separation for modular monoliths
- Implement the read replica pattern with NoTracking
- Handle cross-context transactions with shared connections
- Avoid the 4 most common multi-context mistakes

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

```bash
docker compose up -d
dotnet ef migrations add InitialMovies --context MovieDbContext --output-dir Migrations/MovieDb --project MultiDbContext.Api
dotnet ef migrations add InitialAnalytics --context AnalyticsDbContext --output-dir Migrations/AnalyticsDb --project MultiDbContext.Api
dotnet ef database update --context MovieDbContext --project MultiDbContext.Api
dotnet ef database update --context AnalyticsDbContext --project MultiDbContext.Api
dotnet run --project MultiDbContext.Api
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## Endpoints

| Method | Endpoint | Context | Description |
|--------|----------|---------|-------------|
| GET | `/api/movies` | MovieDbContext | List all movies |
| GET | `/api/movies/{id}` | Both | Get movie + log analytics event |
| POST | `/api/movies` | MovieDbContext | Create a movie |
| GET | `/api/analytics/events` | AnalyticsDbContext | List recent API events |
| POST | `/api/movies/with-event` | Both | Create movie with cross-context transaction |

## Docker Setup

This project runs **two PostgreSQL instances** to demonstrate multi-database scenarios:

| Database | Port | Purpose |
|----------|------|---------|
| movies | 5432 | Domain data (MovieDbContext) |
| analytics | 5433 | Event logs (AnalyticsDbContext) |

## License

MIT
