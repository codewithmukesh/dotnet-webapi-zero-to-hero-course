# Soft Deletes in EF Core 10

Implement soft deletes using EF Core interceptors, named query filters, and cascade delete handling — records are marked as deleted instead of being removed.

## Resources

- **Article**: [Soft Deletes in EF Core 10 - Interceptors, Named Filters & Cascade Delete](https://codewithmukesh.com/blog/soft-deletes-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Implement `ISoftDeletable` interface for soft delete entities
- Use EF Core `SaveChanges` interceptors to automatically handle soft deletes
- Configure named query filters to exclude deleted records by default
- Handle cascade soft deletes for related entities

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

```bash
docker-compose up -d
dotnet run --project SoftDeletes.Api
```

Open the Scalar API docs at the URL shown in the console output.

## License

MIT
