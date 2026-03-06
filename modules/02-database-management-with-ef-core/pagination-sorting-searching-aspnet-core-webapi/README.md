# Pagination, Sorting & Searching in ASP.NET Core Web API

Implement efficient server-side pagination, dynamic sorting, and multi-field search with EF Core and LINQ.

## Resources

- **Article**: [Pagination, Sorting & Searching in ASP.NET Core Web API](https://codewithmukesh.com/blog/pagination-sorting-searching-aspnet-core-webapi/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Implement cursor and offset-based pagination
- Add dynamic sorting with `System.Linq.Dynamic.Core`
- Build multi-field search queries
- Keep all filtering server-side for optimal performance

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL)

## Quick Start

```bash
docker compose up -d
dotnet run --project MovieApi.Api
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## Key Technologies

- Entity Framework Core 10
- System.Linq.Dynamic.Core (dynamic sorting)
- PostgreSQL 17

## License

MIT
