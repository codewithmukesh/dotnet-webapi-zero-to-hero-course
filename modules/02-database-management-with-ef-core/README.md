# Module 02: Database Management with Entity Framework Core

Master data access in ASP.NET Core — from basic CRUD to advanced patterns like bulk operations, soft deletes, and optimistic concurrency control.

Part of the **[.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** course.

## Lessons

| # | Lesson | What You'll Learn | Article | Code |
|---|--------|-------------------|---------|------|
| 1 | Web API CRUD with EF Core | EF Core setup, code-first, migrations, DDD entities, DTOs, service layer | [Read](https://codewithmukesh.com/blog/aspnet-core-webapi-crud-with-entity-framework-core-full-course/) | [Code](./aspnet-core-webapi-crud-with-entity-framework-core-full-course/) |
| 2 | Running Migrations | `Database.MigrateAsync()`, automatic startup migration, schema versioning | [Read](https://codewithmukesh.com/blog/running-migrations-efcore/) | [Code](./running-migrations-efcore/) |
| 3 | Seeding Initial Data | `UseSeeding()`, `UseAsyncSeeding()`, development data initialization | [Read](https://codewithmukesh.com/blog/seeding-initial-data-efcore/) | [Code](./seeding-initial-data-efcore/) |
| 4 | Pagination, Sorting & Searching | Efficient LINQ queries, dynamic sorting, multi-field search, `System.Linq.Dynamic.Core` | [Read](https://codewithmukesh.com/blog/pagination-sorting-searching-aspnet-core-webapi/) | [Code](./pagination-sorting-searching-aspnet-core-webapi/) |
| 5 | Bulk Operations | `ExecuteUpdate`, `ExecuteDelete`, `EFCore.BulkExtensions`, transactional bulk ops | [Read](https://codewithmukesh.com/blog/bulk-operations-efcore/) | [Code](./bulk-operations-efcore/) |
| 6 | Soft Deletes | `ISoftDeletable` interface, EF Core interceptors, named query filters, cascade handling | [Read](https://codewithmukesh.com/blog/soft-deletes-efcore/) | [Code](./soft-deletes-efcore/) |
| 7 | Concurrency Control | Optimistic locking, `RowVersion`, `DbUpdateConcurrencyException`, retry strategies | [Read](https://codewithmukesh.com/blog/concurrency-control-optimistic-locking-efcore/) | [Code](./concurrency-control-optimistic-locking-efcore/) |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL)
- Visual Studio 2022+ / VS Code / JetBrains Rider

## Quick Start

Most lessons in this module require PostgreSQL running in Docker:

```bash
# From any lesson folder with a docker-compose.yml
docker compose up -d

# Run migrations (if needed)
dotnet ef migrations add Initial --project <ProjectName>.Api
dotnet ef database update --project <ProjectName>.Api

# Start the API
dotnet run --project <ProjectName>.Api

# Open API docs at http://localhost:5000/scalar/v1
```

## Database Configuration

All projects connect to PostgreSQL with this default configuration:

| Setting | Value |
|---------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `dotnetHero` |
| Username | `admin` |
| Password | `secret` |

> For production, always use environment variables or a secrets manager for connection strings.

## Tech Stack

All projects in this module use:

- **.NET 10** with Minimal APIs
- **Entity Framework Core 10** (code-first)
- **PostgreSQL 17** (Alpine, via Docker)
- **Scalar** for OpenAPI documentation
