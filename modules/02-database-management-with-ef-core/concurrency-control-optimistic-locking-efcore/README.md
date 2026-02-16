# Concurrency Control in EF Core 10 – Optimistic Locking

Code sample for the article: [Concurrency Control in EF Core 10 – Optimistic Locking with ASP.NET Core Web API](https://codewithmukesh.com/blog/concurrency-control-optimistic-locking-efcore/)

## Quick Start

```bash
docker compose up -d
dotnet ef migrations add Initial --project ConcurrencyControl.Api
dotnet ef database update --project ConcurrencyControl.Api
dotnet run --project ConcurrencyControl.Api
```

Open Scalar API docs at: `http://localhost:5000/scalar/v1`

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/products/seed` | Seed test data |
| GET | `/products` | List all products (includes RowVersion) |
| GET | `/products/{id}` | Get single product |
| PUT | `/products/{id}` | Update product (requires RowVersion — returns 409 on conflict) |
| PATCH | `/products/{id}/stock` | Update stock only (requires RowVersion) |
| PATCH | `/products/{id}/stock-with-retry` | Update stock with automatic retry (last-write-wins) |
| POST | `/products/{id}/simulate-conflict` | Fire 5 concurrent updates to demonstrate conflicts |

## Testing Concurrency

1. Seed data: `POST /products/seed`
2. Simulate conflicts: `POST /products/1/simulate-conflict`
3. Observe that only 1 of 5 concurrent updates succeeds — the rest get `DbUpdateConcurrencyException`
