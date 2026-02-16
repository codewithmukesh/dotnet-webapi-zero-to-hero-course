# Bulk Operations in EF Core 10

Code sample for the article: [Bulk Operations in EF Core 10 – Benchmarking Insert, Update, and Delete Strategies](https://codewithmukesh.com/blog/bulk-operations-efcore/)

## Quick Start

```bash
docker compose up -d
dotnet ef migrations add Initial --project BulkOperations.Api
dotnet ef database update --project BulkOperations.Api
dotnet run --project BulkOperations.Api
```

Open Scalar API docs at: `http://localhost:5000/scalar/v1`

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/products/seed/{count}` | Seed test data |
| POST | `/products/bulk` | Bulk insert with AddRange |
| POST | `/products/bulk-fast` | Bulk insert with EFCore.BulkExtensions |
| PUT | `/products/bulk-update` | Bulk update with ExecuteUpdate |
| PUT | `/products/bulk-deactivate?olderThanDays=90` | Bulk deactivate with ExecuteUpdate |
| DELETE | `/products/bulk-delete/{category}` | Hard delete with ExecuteDelete |
| DELETE | `/products/bulk-soft-delete/{category}` | Soft delete with ExecuteUpdate |
| POST | `/products/bulk-cleanup` | Transactional bulk cleanup |
