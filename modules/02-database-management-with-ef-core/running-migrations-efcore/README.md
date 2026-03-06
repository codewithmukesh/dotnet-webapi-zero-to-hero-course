# Running Migrations in EF Core 10

Manage database schema changes with EF Core migrations — including automatic migration on application startup.

## Resources

- **Article**: [Running Migrations in EF Core 10](https://codewithmukesh.com/blog/running-migrations-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Create and apply EF Core migrations
- Run migrations automatically on startup with `Database.MigrateAsync()`
- Understand when to use `EnsureCreated` vs `Migrate`
- Manage schema versioning across environments

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL)

## Quick Start

```bash
docker compose up -d
dotnet run --project MovieApi.Api
# Migrations are applied automatically on startup
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## License

MIT
