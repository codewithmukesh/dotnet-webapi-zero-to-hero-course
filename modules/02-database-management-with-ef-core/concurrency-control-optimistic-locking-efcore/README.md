# Concurrency Control in EF Core 10 - Optimistic Locking

Handle concurrent database updates safely using optimistic locking with `RowVersion` tokens and `DbUpdateConcurrencyException`.

## Resources

- **Article**: [Concurrency Control in EF Core 10 - Optimistic Locking with ASP.NET Core Web API](https://codewithmukesh.com/blog/concurrency-control-optimistic-locking-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Implement optimistic concurrency with RowVersion tokens
- Handle `DbUpdateConcurrencyException` gracefully
- Build retry strategies for conflict resolution
- Simulate and observe concurrent update conflicts

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

```bash
docker compose up -d
dotnet ef migrations add Initial --project ConcurrencyControl.Api
dotnet ef database update --project ConcurrencyControl.Api
dotnet run --project ConcurrencyControl.Api
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/products/seed` | Seed test data |
| GET | `/products` | List all products (includes RowVersion) |
| GET | `/products/{id}` | Get single product |
| PUT | `/products/{id}` | Update product (requires RowVersion, returns 409 on conflict) |
| PATCH | `/products/{id}/stock` | Update stock only (requires RowVersion) |
| PATCH | `/products/{id}/stock-with-retry` | Update stock with automatic retry (last-write-wins) |
| POST | `/products/{id}/simulate-conflict` | Fire 5 concurrent updates to demonstrate conflicts |

## Testing Concurrency

1. Seed data: `POST /products/seed`
2. Simulate conflicts: `POST /products/1/simulate-conflict`
3. Observe: only 1 of 5 concurrent updates succeeds — the rest get `DbUpdateConcurrencyException`

## License

MIT
