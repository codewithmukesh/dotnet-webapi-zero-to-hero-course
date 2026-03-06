# Seeding Initial Data with EF Core 10

Populate your database with initial data using EF Core's built-in `UseSeeding()` and `UseAsyncSeeding()` callbacks.

## Resources

- **Article**: [Seeding Initial Data with EF Core 10](https://codewithmukesh.com/blog/seeding-initial-data-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Configure seed data with `UseSeeding()` and `UseAsyncSeeding()`
- Trigger seeding during `EnsureCreatedAsync()`
- Manage development vs production seed data
- Avoid duplicate seeding with existence checks

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL)

## Quick Start

```bash
docker compose up -d
dotnet run --project MovieApi.Api
# Seed data is inserted automatically on startup
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## License

MIT
