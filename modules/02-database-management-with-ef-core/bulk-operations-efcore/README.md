# Bulk Operations in EF Core 10

Benchmark and implement high-performance bulk insert, update, and delete strategies — from `AddRange` to `ExecuteUpdate`/`ExecuteDelete` and `EFCore.BulkExtensions`.

## Resources

- **Article**: [Bulk Operations in EF Core 10 - Benchmarking Insert, Update, and Delete Strategies](https://codewithmukesh.com/blog/bulk-operations-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Compare bulk insert strategies (AddRange vs BulkExtensions)
- Use `ExecuteUpdate` and `ExecuteDelete` for efficient server-side operations
- Implement transactional bulk cleanup operations
- Apply soft delete patterns in bulk

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

```bash
docker compose up -d
dotnet ef migrations add Initial --project BulkOperations.Api
dotnet ef database update --project BulkOperations.Api
dotnet run --project BulkOperations.Api
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/products/seed/{count}` | Seed test data |
| POST | `/products/bulk` | Bulk insert with AddRange |
| POST | `/products/bulk-fast` | Bulk insert with EFCore.BulkExtensions |
| PUT | `/products/bulk-update` | Bulk update with ExecuteUpdate |
| PUT | `/products/bulk-deactivate?olderThanDays=90` | Bulk deactivate |
| DELETE | `/products/bulk-delete/{category}` | Hard delete with ExecuteDelete |
| DELETE | `/products/bulk-soft-delete/{category}` | Soft delete with ExecuteUpdate |
| POST | `/products/bulk-cleanup` | Transactional bulk cleanup |

## License

MIT
