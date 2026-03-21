# Module 02: Database Management with Entity Framework Core

Master data access in ASP.NET Core — from basic CRUD to advanced patterns like bulk operations, soft deletes, and optimistic concurrency control.

Part of the **[.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** course.

## Lessons

| # | Lesson | What You'll Learn | Article | Code |
|---|--------|-------------------|---------|------|
| 1 | Web API CRUD with EF Core | EF Core setup, code-first, migrations, DDD entities, DTOs, service layer | [Read](https://codewithmukesh.com/blog/aspnet-core-webapi-crud-with-entity-framework-core-full-course/) | [Code](./aspnet-core-webapi-crud-with-entity-framework-core-full-course/) |
| 2 | EF Core Relationships (1:1, 1:N, M:N) | Fluent API configuration, navigation properties, cascade delete, join tables | [Read](https://codewithmukesh.com/blog/ef-core-relationships-one-to-one-one-to-many-many-to-many/) | — |
| 3 | Pagination, Sorting & Searching | Efficient LINQ queries, dynamic sorting, multi-field search, `System.Linq.Dynamic.Core` | [Read](https://codewithmukesh.com/blog/pagination-sorting-searching-aspnet-core-webapi/) | [Code](./pagination-sorting-searching-aspnet-core-webapi/) |
| 4 | Global Query Filters | Automatic query filtering, soft delete filters, multi-tenancy, `IgnoreQueryFilters` | [Read](https://codewithmukesh.com/blog/global-query-filters-efcore/) | — |
| 5 | Soft Deletes | `ISoftDeletable` interface, EF Core interceptors, named query filters, cascade handling | [Read](https://codewithmukesh.com/blog/soft-deletes-efcore/) | [Code](./soft-deletes-efcore/) |
| 6 | Bulk Operations | `ExecuteUpdate`, `ExecuteDelete`, `EFCore.BulkExtensions`, transactional bulk ops | [Read](https://codewithmukesh.com/blog/bulk-operations-efcore/) | [Code](./bulk-operations-efcore/) |
| 7 | Multiple DbContext | Multi-database setup, schema separation, modular monolith patterns, migration management | [Read](https://codewithmukesh.com/blog/multiple-dbcontext-efcore/) | [Code](./multiple-dbcontext-efcore/) |
| 8 | Concurrency Control | Optimistic locking, `RowVersion`, `DbUpdateConcurrencyException`, retry strategies | Coming Soon | [Code](./concurrency-control-optimistic-locking-efcore/) |
| 9 | Running Migrations | `Database.MigrateAsync()`, automatic startup migration, schema versioning | Coming Soon | [Code](./running-migrations-efcore/) |
| 10 | Seeding Initial Data | `UseSeeding()`, `UseAsyncSeeding()`, development data initialization | Coming Soon | [Code](./seeding-initial-data-efcore/) |
| 11 | Cleaning Migrations | Squash, reset, organize migration history, team conflict resolution | Coming Soon | — |
| 12 | Tracking vs No-Tracking Queries | Change tracker overhead, `AsNoTracking`, `AsNoTrackingWithIdentityResolution`, benchmarks | Coming Soon | — |
| 13 | Compiled Queries | `EF.CompileAsyncQuery`, eliminating translation overhead, hot path optimization | Coming Soon | — |
| 14 | LeftJoin & RightJoin in LINQ | New .NET 10 LINQ join operators, replacing `GroupJoin` workarounds | Coming Soon | — |

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

## Tech Stack

All projects in this module use:

- **.NET 10** with Minimal APIs
- **Entity Framework Core 10** (code-first)
- **PostgreSQL 17** (Alpine, via Docker)
- **Scalar** for OpenAPI documentation
